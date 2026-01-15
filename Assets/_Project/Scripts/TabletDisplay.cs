using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using Meta.WitAi;
using Meta.WitAi.Requests;
using Meta.WitAi.Events;
using Oculus.Voice;
using System;
using UnityEngine.SceneManagement;

public class TabletDisplay : MonoBehaviour
{
    // ============================================================================
    // LAYOUT CONTAINERS
    // ============================================================================
    [Header("Layout Containers")]
    public GameObject idleLayout;
    public GameObject scanLayout;
    // public GameObject lightGameLayout; // not yet implemented

    [Header("Quiz Prompt System")]
    public GameObject quizPromptPanel;

    [Range(0.1f, 1.0f)]
    public float startPercentage = 0.5f;   // İlk hedef (%50)

    [Range(0.01f, 0.2f)]
    public float stepPercentage = 0.05f;   // Artış adımı (%5)

    private float nextTargetRatio;         // Sıradaki hedefi hafızada tutacak

    // ============================================================================
    // UI ELEMENTS - SCAN MODE
    // ============================================================================
    [Header("Scan Mode UI")]
    public TextMeshProUGUI germanLabel;
    public TextMeshProUGUI sentenceText; // Shows AI sentence in scan mode, transcription in pronunciation mode
    public TextMeshProUGUI progressText;
    
    [Header("Scan Mode Buttons")]
    public Button listenButton;
    public Button practicePronunciationButton;

    // ============================================================================
    // UI ELEMENTS - PRONUNCIATION MODE
    // ============================================================================
    [Header("Pronunciation Mode UI")]
    public TextMeshProUGUI pronunciationStatusText;
    public TextMeshProUGUI pronunciationPercentageText;
    
    [Header("Pronunciation Mode Buttons")]
    public Button speakButton;
    public Button listenButtonPronunciation; // Optional: separate button for pronunciation mode

    // ============================================================================
    // AUDIO
    // ============================================================================
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip newDiscoverySound;
    private AudioClip currentAudioClip;

    // ============================================================================
    // GLOBAL UI ELEMENTS
    // ============================================================================
    [Header("Global UI")]
    public TextMeshProUGUI locationLabel;
    public GameObject backgroundGrid;

    // ============================================================================
    // MANAGER REFERENCES
    // ============================================================================
    [Header("Managers")]
    public VocabularyManager vocabManager;
    public SentenceGenerator sentenceGenerator;
    
    // ============================================================================
    // WIT.AI INTEGRATION
    // ============================================================================
    [Header("Wit.ai")]
    [Range(4f, 5f)]
    public float recordingDuration = 4.5f; // Recording duration in seconds
    private AppVoiceExperience voiceExperience; // Found at runtime from VoiceManager GameObject

    // ============================================================================
    // SETTINGS
    // ============================================================================
    [Header("Settings")]
    public string locationName = "KITCHEN";

    // Internal State
    [HideInInspector]
    public bool isProcessing = false; // Lock for processing
    
    // Pronunciation state
    private WordItem currentPronunciationItem; // Store the current item for pronunciation mode
    private WordItem currentScanItem; // Store the currently scanned item
    
    // Wit.ai recording state
    private bool isRecording = false;
    private string expectedText = ""; // The German word user should pronounce
    private Coroutine recordingTimeoutCoroutine;


    void Start()
    {

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();


        //Initialize Globals
        if (locationLabel) locationLabel.text = locationName;
        if (backgroundGrid) backgroundGrid.SetActive(true); // Always on

        // Initialize the counter on startup
        if (progressText != null && VocabularyManager.Instance != null)
        {
            // Note: using your "0/Total" format
            progressText.text = $"0/{VocabularyManager.Instance.GetTotalCount()}";
        }

        nextTargetRatio = startPercentage;

        //Start in idle mode
        SetState(TabletMode.Idle);
        
        // Find AppVoiceExperience at runtime (from VoiceManager GameObject)
        FindVoiceExperience();
        
        // Setup Wit.ai event listeners
        SetupWitAiEvents();
    }
    
    /// <summary>
    /// Finds AppVoiceExperience component at runtime
    /// First tries VoiceManager GameObject, then searches entire scene
    /// </summary>
    private void FindVoiceExperience()
    {
        if (voiceExperience != null) return; // Already found
        
        // Try to find VoiceManager GameObject by name first (most common case)
        GameObject voiceManagerObj = GameObject.Find("VoiceManager");
        if (voiceManagerObj != null)
        {
            voiceExperience = voiceManagerObj.GetComponent<AppVoiceExperience>();
            if (voiceExperience != null)
            {
                Debug.Log("TabletDisplay: Found AppVoiceExperience on VoiceManager GameObject");
                return;
            }
        }
        
        // Fallback: Find any AppVoiceExperience component in scene
        voiceExperience = FindObjectOfType<AppVoiceExperience>();
        if (voiceExperience != null)
        {
            Debug.Log("TabletDisplay: Found AppVoiceExperience component in scene");
        }
        else
        {
            Debug.LogWarning("TabletDisplay: Could not find AppVoiceExperience component. Wit.ai features will not work.");
        }
    }
    
    /// <summary>
    /// Sets up Wit.ai event listeners for transcription
    /// </summary>
    private void SetupWitAiEvents()
    {
        if (voiceExperience != null)
        {
            // Listen for full transcription
            voiceExperience.VoiceEvents.OnFullTranscription.AddListener((text) => OnWitTranscription(text));
            voiceExperience.VoiceEvents.OnPartialTranscription.AddListener((text) => OnWitPartialTranscription(text));
            voiceExperience.VoiceEvents.OnError.AddListener(OnWitError);
            voiceExperience.VoiceEvents.OnRequestCompleted.AddListener(OnWitRequestCompleted);
        }
        else
        {
            Debug.LogWarning("TabletDisplay: AppVoiceExperience not assigned. Wit.ai features will not work.");
        }
    }

    private void OnWitRequestCompleted()
    {
        throw new NotImplementedException();
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (voiceExperience != null)
        {
            voiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnWitTranscription);
            voiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnWitPartialTranscription);
            voiceExperience.VoiceEvents.OnError.RemoveListener(OnWitError);
            voiceExperience.VoiceEvents.OnRequestCompleted.RemoveListener(OnWitRequestCompleted);
        }
    }

    public enum TabletMode { Idle, Scanning, Pronunciation }

    public void SetState(TabletMode mode)
    {
        //Reset: Turn off all dynamic layouts
        if (idleLayout) idleLayout.SetActive(false);
        if (scanLayout) scanLayout.SetActive(false);

        //Activate the layout
        switch (mode)
        {
            case TabletMode.Idle:
                if (idleLayout) idleLayout.SetActive(true);
                break;

            case TabletMode.Scanning:
                if (scanLayout) scanLayout.SetActive(true);
                // Show scan mode elements
                ShowScanModeElements();
                break;

            case TabletMode.Pronunciation:
                if (scanLayout) scanLayout.SetActive(true); // Use same layout
                // Show pronunciation mode elements
                ShowPronunciationModeElements();
                break;
        }
    }
    
    /// <summary>
    /// Shows elements for scan mode
    /// </summary>
    private void ShowScanModeElements()
    {
        // Show scan mode UI
        if (sentenceText != null)
        {
            sentenceText.gameObject.SetActive(true);
            // Reset sentence text to show generated sentence (not pronunciation transcription)
        }
        if (practicePronunciationButton != null)
        {
            practicePronunciationButton.gameObject.SetActive(true);
            practicePronunciationButton.interactable = (currentScanItem != null);
        }
        
        // Show buttons in scan mode
        if (listenButton != null)
        {
            listenButton.gameObject.SetActive(true);
            listenButton.interactable = (currentAudioClip != null);
        }
        if (speakButton != null)
        {
            speakButton.gameObject.SetActive(true);
            speakButton.interactable = (currentScanItem != null);
        }
        
        // Hide pronunciation mode UI
        if (pronunciationStatusText != null) pronunciationStatusText.gameObject.SetActive(false);
        if (pronunciationPercentageText != null) pronunciationPercentageText.gameObject.SetActive(false);
        if (listenButtonPronunciation != null) listenButtonPronunciation.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Shows elements for pronunciation mode
    /// </summary>
    private void ShowPronunciationModeElements()
    {
        // Keep sentenceText visible - it will show the transcribed text from Wit.ai
        if (sentenceText != null)
        {
            sentenceText.gameObject.SetActive(true);
            sentenceText.text = ""; // Clear it initially, will be filled with transcription
        }
        
        // Hide scan mode UI
        if (practicePronunciationButton != null) practicePronunciationButton.gameObject.SetActive(false);
        if (listenButton != null) listenButton.gameObject.SetActive(false);
        
        // Show pronunciation mode UI
        ShowPronunciationReady();
    }


    // SCANNING LOOP
    // This is called by the Right Hand Scanner
    public void UpdateDisplay(WordItem item)
    {
        if (isProcessing) return;
        isProcessing = true;
        currentScanItem = item;
        SetState(TabletMode.Scanning);

        StopAllCoroutines();
        if (audioSource) audioSource.Stop();

        // Ses dosyasını yükleme işlemleri...
        if (currentAudioClip != null)
        {
            Resources.UnloadAsset(currentAudioClip);
            currentAudioClip = null;
        }

        string audioPath = "Audios/" + item.objectID;
        currentAudioClip = Resources.Load<AudioClip>(audioPath);

        if (listenButton != null) listenButton.interactable = (currentAudioClip != null);

        // --- YENİ KEŞİF VE YÜZDE HESAPLAMA KISMI ---
        bool isNew = !SentenceHistoryManager.IsDiscovered(item.objectID);

        if (isNew)
        {
            SentenceHistoryManager.MarkAsDiscovered(item.objectID);
            if (newDiscoverySound != null) audioSource.PlayOneShot(newDiscoverySound);

            // BURAYA EKLEME YAPIYORUZ: YÜZDE KONTROLÜ
            CheckProgressForQuiz();
        }

        if (germanLabel != null) germanLabel.text = item.germanWord;

        UpdateProgressUI();
        if (sentenceText) sentenceText.text = "Generating sentence...";

        if (sentenceGenerator != null)
        {
            sentenceGenerator.RequestSentence(item, (result) =>
            {
                if (sentenceText) sentenceText.text = result;
                isProcessing = false;
            });
        }
        else
        {
            isProcessing = false;
        }
    }

    // --- YENİ EKLENEN FONKSİYONLAR ---

    void CheckProgressForQuiz()
    {
        if (VocabularyManager.Instance == null) return;

        float found = SentenceHistoryManager.GetDiscoveredCount();
        float total = VocabularyManager.Instance.GetTotalCount();

        if (total == 0) return;

        float currentRatio = found / total;

        // Eğer mevcut oran, sıradaki hedefi geçtiyse (veya eşitse)
        if (currentRatio >= nextTargetRatio)
        {
            ShowQuizPrompt();

            // HEDEFİ YÜKSELT
            // Örn: 0.50 -> 0.55 -> 0.60
            while (nextTargetRatio <= currentRatio)
            {
                nextTargetRatio += stepPercentage;
            }

            // %100'ü geçerse tavan yap
            if (nextTargetRatio > 1.0f) nextTargetRatio = 1.01f;
        }
    }

    void ShowQuizPrompt()
    {
        if (quizPromptPanel != null)
        {
            // 1. Arkadaki Scan Panelini GİZLE
            if (scanLayout != null) scanLayout.SetActive(false);

            // 2. Quiz Teklif Panelini AÇ
            quizPromptPanel.SetActive(true);

        }
    }

    // Butona bağlanacak fonksiyon: Quiz Sahnesine Git
    public void GoToQuizScene()
    {
        // Session verisini güncelle (İsteğe bağlı)
        if (GameSession.Instance != null)
        {
            GameSession.Instance.SelectedLevel = "QuizMode";
        }

        Debug.Log("Teleporting to Kitchen_Quiz...");
        SceneManager.LoadScene("Kitchen_Quiz");
    }

    // "Daha Sonra" / "Back" butonuna bağlı fonksiyon
    public void CloseQuizPrompt()
    {
        if (quizPromptPanel != null)
        {
            // 1. Quiz Teklif Panelini GİZLE
            quizPromptPanel.SetActive(false);

            // 2. Eski Scan Panelini geri GETİR
            if (scanLayout != null) scanLayout.SetActive(true);
        }
    }

    void UpdateProgressUI()
    {
        if (progressText != null && VocabularyManager.Instance != null)
        {
            int found = SentenceHistoryManager.GetDiscoveredCount();
            int total = VocabularyManager.Instance.GetTotalCount();
            progressText.text = $"{found}/{total}";
        }
    }
    public void OnListenButtonPressed()
    {
        PlayAudioWithButton(listenButton);
    }
    
    /// <summary>
    /// Shared method to play audio with any button - eliminates code duplication
    /// </summary>
    private void PlayAudioWithButton(Button buttonToUse)
    {
        if (audioSource && currentAudioClip)
        {
            StartCoroutine(PlayAudioRoutine(buttonToUse));
        }
    }
    
    private System.Collections.IEnumerator PlayAudioRoutine(Button buttonToUse)
    {
        // Visual feedback that it's "working"
        if (buttonToUse) buttonToUse.interactable = false;

        audioSource.Stop(); // Clean start
        audioSource.PlayOneShot(currentAudioClip);

        // (plus a tiny 0.1s buffer so it feels smooth)
        yield return new WaitForSeconds(currentAudioClip.length + 0.1f);

        // Safety Check
        if (buttonToUse != null && currentAudioClip != null)
        {
            buttonToUse.interactable = true;
        }
    }

    // ============================================================================
    // PRONUNCIATION LAYOUT METHODS
    // ============================================================================

    /// <summary>
    /// Switches to pronunciation mode for the current word
    /// </summary>
    public void EnterPronunciationMode(string wordToPronounce)
    {
        // Word is already shown in germanLabel, no need for separate text
        SetState(TabletMode.Pronunciation);
    }

    /// <summary>
    /// Enters pronunciation mode using the currently scanned item
    /// Call this from the practicePronunciationButton in the scan layout
    /// This is the main entry point from scan mode to pronunciation mode
    /// </summary>
    public void EnterPronunciationModeFromScan()
    {
        if (currentScanItem != null)
        {
            EnterPronunciationMode(currentScanItem);
        }
        else if (germanLabel != null && !string.IsNullOrEmpty(germanLabel.text))
        {
            // Fallback: if no item stored, try to use the displayed word
            Debug.LogWarning("No WordItem stored, using displayed word as fallback");
            EnterPronunciationMode(germanLabel.text);
        }
        else
        {
            Debug.LogWarning("No word currently available to pronounce. Please scan an object first.");
        }
    }

    /// <summary>
    /// Enters pronunciation mode using the currently displayed German word (legacy method)
    /// </summary>
    public void EnterPronunciationModeForCurrentWord()
    {
        EnterPronunciationModeFromScan();
    }

    /// <summary>
    /// Enters pronunciation mode with a WordItem (preserves audio reference)
    /// This is the recommended way to enter pronunciation mode
    /// </summary>
    public void EnterPronunciationMode(WordItem item)
    {
        currentPronunciationItem = item;
        
        // Word is already shown in germanLabel from scan mode, no need to set it again
        
        // Load audio for this item (same as scan mode)
        if (item != null)
        {
            string audioPath = "Audios/" + item.objectID;
            currentAudioClip = Resources.Load<AudioClip>(audioPath);
        }
        
        SetState(TabletMode.Pronunciation);
    }

    /// <summary>
    /// Shows the initial state with speak button ready
    /// </summary>
    private void ShowPronunciationReady()
    {
        // Show speak button
        if (speakButton != null)
        {
            speakButton.gameObject.SetActive(true);
            speakButton.interactable = true;
        }

        // Show listen button (if audio is available)
        Button listenBtn = GetPronunciationListenButton();
        if (listenBtn != null)
        {
            listenBtn.gameObject.SetActive(true);
            listenBtn.interactable = (currentAudioClip != null);
        }

        // Hide percentage initially
        if (pronunciationPercentageText != null)
        {
            pronunciationPercentageText.gameObject.SetActive(false);
        }

        // Show status text
        if (pronunciationStatusText != null)
        {
            if (currentAudioClip != null)
            {
                pronunciationStatusText.text = "Listen first, then practice speaking";
            }
            else
            {
                pronunciationStatusText.text = "Press the button to speak";
            }
            pronunciationStatusText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Called when speak button is pressed - starts recording
    /// </summary>
    public void OnSpeakButtonPressed()
    {
        if (isRecording)
        {
            Debug.LogWarning("Already recording!");
            return;
        }

        // Get the expected text (German word without article)
        if (currentScanItem != null)
        {
            expectedText = currentScanItem.germanWord;
            // Remove article if present (e.g., "Die Kaffeetasse" -> "Kaffeetasse")
            if (expectedText.Contains(" "))
            {
                string[] parts = expectedText.Split(' ');
                if (parts.Length > 1)
                {
                    expectedText = string.Join(" ", parts, 1, parts.Length - 1);
                }
            }
        }
        else if (germanLabel != null)
        {
            expectedText = germanLabel.text;
        }

        // Disable both buttons during recording
        SetButtonInteractable(speakButton, false);
        SetButtonInteractable(GetPronunciationListenButton(), false);

        // Hide previous result when starting a new recording
        if (pronunciationPercentageText != null)
        {
            pronunciationPercentageText.gameObject.SetActive(false);
        }

        // Clear sentenceText to show transcription
        if (sentenceText != null)
        {
            sentenceText.text = "";
        }

        if (pronunciationStatusText != null)
        {
            pronunciationStatusText.text = "Speaking...";
        }

        // Start Wit.ai recording
        StartWitRecording();
    }
    
    /// <summary>
    /// Starts Wit.ai voice recording
    /// </summary>
    private void StartWitRecording()
    {
        if (voiceExperience == null)
        {
            Debug.LogError("Wit.ai AppVoiceExperience not assigned!");
            OnRecordingError("Wit.ai not configured");
            return;
        }

        isRecording = true;
        
        // Activate Wit.ai to start recording
        voiceExperience.Activate();
        
        // Start timeout coroutine (stop after recordingDuration seconds)
        if (recordingTimeoutCoroutine != null)
        {
            StopCoroutine(recordingTimeoutCoroutine);
        }
        recordingTimeoutCoroutine = StartCoroutine(RecordingTimeoutCoroutine());
        
        Debug.Log($"Started recording for {recordingDuration} seconds. Expected: {expectedText}");
    }
    
    /// <summary>
    /// Stops recording after timeout
    /// </summary>
    private System.Collections.IEnumerator RecordingTimeoutCoroutine()
    {
        yield return new WaitForSeconds(recordingDuration);
        
        if (isRecording)
        {
            StopWitRecording();
        }
    }
    
    /// <summary>
    /// Stops Wit.ai recording
    /// </summary>
    private void StopWitRecording()
    {
        if (voiceExperience != null && isRecording)
        {
            voiceExperience.Deactivate();
            isRecording = false;
            
            if (pronunciationStatusText != null)
            {
                pronunciationStatusText.text = "Processing...";
            }
        }
    }
    
    /// <summary>
    /// Called when Wit.ai provides partial transcription (while speaking)
    /// </summary>
    private void OnWitPartialTranscription(string transcription)
    {
        if (isRecording && sentenceText != null)
        {
            sentenceText.text = transcription;
        }
    }
    
    /// <summary>
    /// Called when Wit.ai provides full transcription
    /// </summary>
    private void OnWitTranscription(string transcription)
    {
        if (!isRecording) return;
        
        Debug.Log($"Wit.ai Transcription: {transcription}");
        
        // Update the sentenceText with transcription
        UpdatePronunciationTranscription(transcription);
        
        // Calculate pronunciation score
        float score = CalculatePronunciationScore(transcription, expectedText);
        
        // Show the result
        ShowPronunciationResult(score);
        
        isRecording = false;
        
        // Stop timeout coroutine
        if (recordingTimeoutCoroutine != null)
        {
            StopCoroutine(recordingTimeoutCoroutine);
            recordingTimeoutCoroutine = null;
        }
    }
    
    /// <summary>
    /// Called when Wit.ai request is completed
    /// </summary>
    private void OnWitRequestCompleted(VoiceServiceRequest request)
    {
        if (isRecording)
        {
            isRecording = false;
        }
    }
    
    /// <summary>
    /// Called when Wit.ai encounters an error
    /// </summary>
    private void OnWitError(string error, string message)
    {
        Debug.LogError($"Wit.ai Error: {error} - {message}");
        OnRecordingError(message);
    }
    
    /// <summary>
    /// Handles recording errors
    /// </summary>
    private void OnRecordingError(string errorMessage)
    {
        isRecording = false;
        
        // Re-enable buttons
        SetButtonInteractable(speakButton, true);
        SetButtonInteractable(GetPronunciationListenButton(), currentAudioClip != null);
        
        if (pronunciationStatusText != null)
        {
            pronunciationStatusText.text = "Error: " + errorMessage + ". Try again.";
        }
        
        if (sentenceText != null)
        {
            sentenceText.text = "";
        }
    }
    
    /// <summary>
    /// Calculates pronunciation accuracy score (0-100%)
    /// Compares transcribed text with expected German word
    /// </summary>
    private float CalculatePronunciationScore(string transcribed, string expected)
    {
        if (string.IsNullOrEmpty(transcribed) || string.IsNullOrEmpty(expected))
        {
            return 0f;
        }
        
        // Normalize strings (lowercase, remove extra spaces)
        transcribed = transcribed.Trim().ToLower();
        expected = expected.Trim().ToLower();
        
        // Exact match = 100%
        if (transcribed == expected)
        {
            return 100f;
        }
        
        // Check if transcribed contains expected word
        if (transcribed.Contains(expected))
        {
            return 90f; // Very close
        }
        
        // Calculate similarity using Levenshtein distance
        int distance = LevenshteinDistance(transcribed, expected);
        int maxLength = Mathf.Max(transcribed.Length, expected.Length);
        
        if (maxLength == 0) return 0f;
        
        float similarity = 1f - ((float)distance / maxLength);
        return Mathf.Clamp(similarity * 100f, 0f, 100f);
    }
    
    /// <summary>
    /// Calculates Levenshtein distance between two strings
    /// </summary>
    private int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;
        
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        
        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }
        
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        
        return d[n, m];
    }
    
    /// <summary>
    /// Updates the sentenceText with the transcribed text from Wit.ai
    /// Call this when you receive the transcription from Wit.ai
    /// </summary>
    public void UpdatePronunciationTranscription(string transcribedText)
    {
        if (sentenceText != null)
        {
            sentenceText.text = transcribedText;
        }
    }

    /// <summary>
    /// Called when pronunciation is complete - shows the percentage result
    /// After showing result, both listen and speak buttons remain available for repeated practice
    /// The transcribed text should already be in sentenceText from UpdatePronunciationTranscription()
    /// </summary>
    public void ShowPronunciationResult(float percentage)
    {
        // Keep both buttons available for repeated practice
        if (speakButton != null)
        {
            speakButton.gameObject.SetActive(true);
            speakButton.interactable = true;
        }

        // Re-enable listen button
        SetButtonInteractable(GetPronunciationListenButton(), currentAudioClip != null);

        // Show percentage
        if (pronunciationPercentageText != null)
        {
            pronunciationPercentageText.gameObject.SetActive(true);
            pronunciationPercentageText.text = $"{percentage:F0}%";
            
            // Color code based on percentage
            if (percentage < 60f)
            {
                pronunciationPercentageText.color = Color.red;
            }
            else if (percentage < 80f)
            {
                pronunciationPercentageText.color = Color.yellow;
            }
            else
            {
                pronunciationPercentageText.color = Color.green;
            }
        }

        // Update status text - encourage continued practice
        if (pronunciationStatusText != null)
        {
            if (percentage >= 80f)
            {
                pronunciationStatusText.text = "Excellent! Listen again or practice more.";
            }
            else if (percentage >= 60f)
            {
                pronunciationStatusText.text = "Good! Try again for better score.";
            }
            else
            {
                pronunciationStatusText.text = "Keep practicing! Listen and try again.";
            }
        }
        
        // sentenceText already contains the transcribed text from Wit.ai
        // It will remain visible showing what the user said
    }

    /// <summary>
    /// Allows user to retry pronunciation
    /// </summary>
    public void RetryPronunciation()
    {
        ShowPronunciationReady();
    }

    /// <summary>
    /// Override for listen button in pronunciation mode
    /// Ensures audio plays correctly and doesn't interfere with pronunciation flow
    /// </summary>
    public void OnListenButtonPressedInPronunciation()
    {
        if (audioSource && currentAudioClip)
        {
            StartCoroutine(PlayAudioInPronunciationMode());
        }
    }

    private System.Collections.IEnumerator PlayAudioInPronunciationMode()
    {
        // Get the correct listen button for pronunciation mode
        Button listenBtn = GetPronunciationListenButton();
        
        // Disable buttons during playback
        SetButtonInteractable(listenBtn, false);
        SetButtonInteractable(speakButton, false);

        if (pronunciationStatusText != null)
        {
            pronunciationStatusText.text = "Playing audio...";
        }

        audioSource.Stop();
        audioSource.PlayOneShot(currentAudioClip);

        yield return new WaitForSeconds(currentAudioClip.length + 0.1f);

        // Re-enable buttons after playback
        SetButtonInteractable(listenBtn, currentAudioClip != null);
        SetButtonInteractable(speakButton, true);

        if (pronunciationStatusText != null)
        {
            pronunciationStatusText.text = "Now try speaking!";
        }
    }
    
    /// <summary>
    /// Helper method to get the correct listen button for pronunciation mode
    /// </summary>
    private Button GetPronunciationListenButton()
    {
        return listenButtonPronunciation != null ? listenButtonPronunciation : listenButton;
    }
    
    /// <summary>
    /// Helper method to safely set button interactable state
    /// </summary>
    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }
}