using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class TabletDisplay : MonoBehaviour
{


    [Header("Layout Containers")]
    public GameObject idleLayout;
    public GameObject scanLayout;

    // public GameObject lightGameLayout; // not yet implemented

    [Header("Scan Layout Content")]
    public TextMeshProUGUI germanLabel;
    public TextMeshProUGUI sentenceText;
    public TextMeshProUGUI progressText;

    [Header("Buttons")]
    public Button listenButton;

    [Header("Audio Settings")]
    public AudioClip newDiscoverySound; // Audio when a new object discovered
    public AudioSource audioSource;   // Global audio source for everything basically
    private AudioClip currentAudioClip;

    [Header("Global Elements")]
    public TextMeshProUGUI locationLabel;
    public GameObject backgroundGrid;

    [Header("Manager Connections")]
    public VocabularyManager vocabManager;
    public SentenceGenerator sentenceGenerator; // Reference for llm-sentence generator

    [Header("Settings")]
    public string locationName = "KITCHEN";

    // Internal State
    [HideInInspector]
    public bool isProcessing = false; // Lock for processing


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

        //Start in idle mode
        SetState(TabletMode.Idle);
    }

    public enum TabletMode { Idle, Scanning }

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
                break;
        }
    }


    // SCANNING LOOP
    // This is called by the Right Hand Scanner
    public void UpdateDisplay(WordItem item)
    {
        if (isProcessing) return;

        // Lock
        isProcessing = true;

        // Switch UI to Scan Mode ---
        SetState(TabletMode.Scanning);


        StopAllCoroutines();
        if (audioSource) audioSource.Stop();

        //Clean up memory by unloading the previous clip
        if (currentAudioClip != null)
        {
            Resources.UnloadAsset(currentAudioClip);
            currentAudioClip = null;
        }

        string audioPath = "Audios/" + item.objectID;
        Debug.LogWarning($"{audioPath} found");
        currentAudioClip = Resources.Load<AudioClip>(audioPath);

        if (currentAudioClip == null)
        {
            Debug.LogWarning($"Audio not found for: {audioPath}");
            if (listenButton) listenButton.interactable = false; // Disable button
        }
        else
        {
            if (listenButton) listenButton.interactable = true; // Enable button
        }

        if (listenButton != null)
        {
            listenButton.interactable = (currentAudioClip != null);
        }





        //Check whether the object discovered or new 
        bool isNew = !SentenceHistoryManager.IsDiscovered(item.objectID);

        if (isNew && newDiscoverySound != null)
        {
            SentenceHistoryManager.MarkAsDiscovered(item.objectID);
            audioSource.PlayOneShot(newDiscoverySound);
        }

        if (germanLabel != null)
        {
            // Get the German word from the scanned item
            string textToShow = item.germanWord;

            // Update the screen
            germanLabel.text = textToShow;
        }
        else
        {
            Debug.LogError("Tablet Error: GermanLabel not linked");
            isProcessing = false;
        }

        UpdateProgressUI();
        // Generate Text
        if (sentenceText) sentenceText.text = "Generating sentence...";



        if (sentenceGenerator != null)
        {
            // Small function (lambda) that runs when the AI finishes
            sentenceGenerator.RequestSentence(item, (result) =>
            {
                if (sentenceText) sentenceText.text = result;

                // Unlock when done
                isProcessing = false;
            });
        }
        else
        {
            Debug.LogError("SentenceGenerator is not linked");
            isProcessing = false; // Unlock if we fail so game doesn't freeze
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
        if (audioSource && currentAudioClip)
        {
            StartCoroutine(PlayAudioRoutine());
        }
    }
    private System.Collections.IEnumerator PlayAudioRoutine()
    {
        //Visual feedback that it's "working"
        if (listenButton) listenButton.interactable = false;


        audioSource.Stop(); // Clean start
        audioSource.PlayOneShot(currentAudioClip);

        // (plus a tiny 0.1s buffer so it feels smooth)
        yield return new WaitForSeconds(currentAudioClip.length + 0.1f);

        // Safety Check
        if (listenButton != null && currentAudioClip != null)
        {
            listenButton.interactable = true;
        }
    }
}