using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class QuizGameManager : MonoBehaviour
{
    public static QuizGameManager Instance;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject inputPanel;
    public GameObject endPanel;

    [Header("UI References")]
    public TMP_InputField nameInput;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI finalTimeText;

    [Header("Leaderboard List UI")]
    public Transform leaderboardContainer; // Sat�rlar�n dizilece�i kutu
    public GameObject scoreRowPrefab;      // Sat�r �ablonu

    [Header("Name Input (Pre-filled 'Player')")]
    public string defaultPlayerName = "Player";

    [Header("Game Settings")]
    public int totalQuestions = 5;
    public Color correctColor = Color.green;
    public Color penaltyColor = Color.red;

    // --- Music ---
    [Header("Audio")]
    public AudioSource challengeMusicSource; // M�zik kayna�� buraya
    public bool IsGameActive { get; private set; } = false;
    private float currentTime = 0f;
    private List<ItemData> questionQueue = new List<ItemData>();
    private ItemData currentTarget;
    
    // --- External Challenge Mode ---
    private bool isExternalChallenge = false;
    private List<string> externalItemIDs = new List<string>();
    private bool challengeTimed = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Don't auto-show any panel - wait for explicit start
        // Standalone mode: call StartGame() from a button
        // Integrated mode: GameFlowController calls StartExternalChallenge()
        ShowPanel(null); // Hide all panels at start
    }
    
    /// <summary>
    /// Called by GameFlowController to start challenge from exploration
    /// </summary>
    public void StartExternalChallenge(List<string> itemIDs, bool timed)
    {
        isExternalChallenge = true;
        externalItemIDs = new List<string>(itemIDs);
        challengeTimed = timed;
        
        // Re-enable scanning so player can find objects during challenge
        RightHandScanner.CanScan = true;
        
        // Skip start panel, go directly to game
        currentTime = 0f;
        PrepareExternalQuestions();
        IsGameActive = true;
        ShowPanel(gamePanel);
        AskNextQuestion();

        // Start music
        if (challengeMusicSource != null)
        {
            challengeMusicSource.volume = 0.5f;
            challengeMusicSource.Play();
        }
    }
    
    void PrepareExternalQuestions()
    {
        if (VocabularyManager.Instance == null) return;

        questionQueue.Clear();
        
        // Use only the provided item IDs
        foreach (string id in externalItemIDs)
        {
            ItemData item = VocabularyManager.Instance.GetItem(id);
            if (item != null)
            {
                questionQueue.Add(item);
            }
        }
        
        // Shuffle
        for (int i = 0; i < questionQueue.Count; i++)
        {
            ItemData temp = questionQueue[i];
            int randomIndex = Random.Range(i, questionQueue.Count);
            questionQueue[i] = questionQueue[randomIndex];
            questionQueue[randomIndex] = temp;
        }
    }

    void Update()
    {
        if (IsGameActive)
        {
            currentTime += Time.deltaTime;
            float minutes = Mathf.FloorToInt(currentTime / 60);
            float seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // --- OYUN AKI�I ---

    public void StartGame()
    {
        currentTime = 0f;
        PrepareQuestions();
        IsGameActive = true;
        ShowPanel(gamePanel);
        AskNextQuestion();

        // --- start the music ---
        if (challengeMusicSource != null)
        {
            challengeMusicSource.volume = 0.5f; // Ses seviyesini ayarla (iste�e ba�l�)
            challengeMusicSource.Play();
        }
    }

    void PrepareQuestions()
    {
        if (VocabularyManager.Instance == null) return;

        List<ItemData> allItems = new List<ItemData>(VocabularyManager.Instance.database.Values);

        // Kar��t�r
        for (int i = 0; i < allItems.Count; i++)
        {
            ItemData temp = allItems[i];
            int randomIndex = Random.Range(i, allItems.Count);
            allItems[i] = allItems[randomIndex];
            allItems[randomIndex] = temp;
        }

        questionQueue = allItems.GetRange(0, Mathf.Min(totalQuestions, allItems.Count));
    }

    void AskNextQuestion()
    {
        if (questionQueue.Count > 0)
        {
            currentTarget = questionQueue[0];
            questionQueue.RemoveAt(0);
            questionText.text = "Find: " + currentTarget.german;
        }
        else
        {
            EndGame();
        }
    }

    public void SubmitAnswer(string scannedID)
    {
        if (!IsGameActive) return;

        if (scannedID == currentTarget.id)
        {
            currentTime -= 2f;
            if (currentTime < 0) currentTime = 0;
            StartCoroutine(ShowFeedback("Correct! (-2 sec)", correctColor));
            
            // Play correct sound
            RightHandScanner.Instance?.PlayFeedbackSound(true);
            
            AskNextQuestion();
        }
        else
        {
            currentTime += 5f;
            StartCoroutine(ShowFeedback("Wrong! (+5 sec)", penaltyColor));
            
            // Play wrong sound
            RightHandScanner.Instance?.PlayFeedbackSound(false);
        }
    }

    IEnumerator ShowFeedback(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        yield return new WaitForSeconds(1.5f);
        feedbackText.text = "";
    }

    void EndGame()
    {
        IsGameActive = false;
        if (finalTimeText) finalTimeText.text = $"Your Time: {currentTime:F1} sec";
        
        // --- stop the music ---
        if (challengeMusicSource != null)
        {
            challengeMusicSource.Stop();
        }
        
        // Lock scanning while in InputPanel (prevent scan/layout overlap)
        RightHandScanner.CanScan = false;
        
        // Show InputPanel first for name entry (pre-filled with "Player")
        SetupNameInput();
        ShowPanel(inputPanel);
    }
    
    /// <summary>
    /// Called by EndPanel retry button - play again
    /// </summary>
    public void OnRetryChallenge()
    {
        Debug.Log("[QuizGameManager] Retry clicked - restarting challenge");
        // Fully reset state
        currentTime = 0f;
        questionQueue.Clear();
        currentTarget = null;
        feedbackText.text = "";
        
        // Start fresh (EndPanel hid the panel, now restart challenge)
        StartExternalChallenge(externalItemIDs, challengeTimed);
    }
    
    /// <summary>
    /// Scan any object to dismiss EndPanel and return to exploration
    /// </summary>
    public void OnContinueExploring()
    {
        Debug.Log("[QuizGameManager] Continue triggered");
        ReturnToExploration();
    }
    
    /// <summary>
    /// Call this from RightHandScanner when player scans during EndPanel
    /// </summary>
    public bool TryDismissEndPanel()
    {
        // Only works if EndPanel is currently showing
        if (endPanel != null && endPanel.activeSelf && isExternalChallenge)
        {
            Debug.Log("[QuizGameManager] Scan detected during EndPanel - dismissing");
            ReturnToExploration();
            return true; // Tell scanner we handled it
        }
        return false; // Let scanner process normally
    }
    
    void ReturnToExploration()
    {
        // Re-enable scanning for exploration
        RightHandScanner.CanScan = true;
        
        // Hide end panel
        ShowPanel(null);
        
        // Notify GameFlowController that challenge is done
        isExternalChallenge = false;
        GameFlowController.Instance?.OnFindChallengeComplete(currentTime);
    }
    
    /// <summary>
    /// Called by GameFlowController to cancel the challenge
    /// </summary>
    public void CancelChallenge()
    {
        IsGameActive = false;
        isExternalChallenge = false;
        
        if (challengeMusicSource != null)
        {
            challengeMusicSource.Stop();
        }
        
        ShowPanel(null);
        GameFlowController.Instance?.OnFindChallengeCancelled();
    }

    public void RestartGame()
    {
        ShowPanel(startPanel);
    }

    // --- SKOR VE L�DERL�K TABLOSU ---

    /// <summary>
    /// Call this when showing InputPanel to pre-fill name
    /// </summary>
    public void SetupNameInput()
    {
        if (nameInput != null)
        {
            nameInput.text = defaultPlayerName;
        }
    }
    
    public void SubmitScore()
    {
        // Get name from input (default is "Player", user can change if VR keyboard added)
        string playerName = defaultPlayerName;
        if (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
        {
            playerName = nameInput.text;
        }
        
        // Save score
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SaveScore(playerName, currentTime);
            Debug.Log($"[QuizGameManager] Saved score for {playerName}: {currentTime:F1} sec");
        }
        
        // Re-enable scanning for EndPanel (scan-to-continue feature)
        RightHandScanner.CanScan = true;
        
        // Show EndPanel with leaderboard
        ShowPanel(endPanel);
        
        // Update leaderboard display after panel is active
        UpdateLeaderboardDisplay();
    }

    public void UpdateLeaderboardDisplay()
    {
        Debug.Log("[QuizGameManager] UpdateLeaderboardDisplay called");
        
        if (leaderboardContainer == null)
        {
            Debug.LogError("[QuizGameManager] leaderboardContainer is NULL! Check QuizGameManager inspector.");
            return;
        }
        if (scoreRowPrefab == null)
        {
            Debug.LogError("[QuizGameManager] scoreRowPrefab is NULL! Assign ScoreRowTemplate prefab.");
            return;
        }
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("[QuizGameManager] LeaderboardManager.Instance is NULL! Add LeaderboardManager to scene.");
            return;
        }
        
        Debug.Log($"[QuizGameManager] leaderboardContainer: {leaderboardContainer.name}, scoreRowPrefab: {scoreRowPrefab.name}");
        
        // 1. Clear previous list
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        LeaderboardData data = LeaderboardManager.Instance.LoadScores();
        Debug.Log($"[QuizGameManager] Loaded {data.scores.Count} scores from leaderboard");

        if (data.scores.Count == 0)
        {
            Debug.Log("[QuizGameManager] No scores to display");
            return;
        }

        // 2. Create new list
        for (int i = 0; i < data.scores.Count; i++)
        {
            Debug.Log($"[QuizGameManager] Creating row {i+1}: {data.scores[i].playerName} - {data.scores[i].timeScore}");
            GameObject row = Instantiate(scoreRowPrefab, leaderboardContainer);
            SetupScoreRow(row, i, data.scores[i]);
        }
        
        Debug.Log("[QuizGameManager] Leaderboard display updated successfully");
    }

    // Yard�mc� fonksiyon: Her sat�r� ve butonu izole eder
    void SetupScoreRow(GameObject rowObj, int index, ScoreEntry entry)
    {
        // Yaz�y� Ayarla
        TextMeshProUGUI[] texts = rowObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 1)
        {
            texts[0].text = $"{index + 1}. {entry.playerName} - {entry.timeScore:F1} sec";
        }

        // Butonu Ayarla
        Button deleteBtn = rowObj.GetComponentInChildren<Button>();
        if (deleteBtn != null)
        {
            // Eski ba�lant�lar� temizle
            deleteBtn.onClick.RemoveAllListeners();

            // Yeni silme emrini ver (index de�eri burada sabitlenmi�tir)
            deleteBtn.onClick.AddListener(() =>
            {
                Debug.Log($"[QuizManager] Silme butonuna bas�ld�. Silinecek index: {index}");
                DeleteSingleScore(index-1);
            });
        }
    }

    public void DeleteSingleScore(int index)
    {
        Debug.Log($"[QuizManager] DeleteSingleScore �al��t�. Index: {index}");
        LeaderboardManager.Instance.DeleteScoreAtIndex(index);
        UpdateLeaderboardDisplay(); // Listeyi yenile
    }

    public void ClearAllLeaderboard()
    {
        Debug.Log("[QuizGameManager] Clear All clicked - clearing scores");
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.ClearAllScores();
            Debug.Log("[QuizGameManager] Scores cleared, refreshing display");
            UpdateLeaderboardDisplay();
        }
        else
        {
            Debug.LogError("[QuizGameManager] LeaderboardManager.Instance is null!");
        }
    }

    // --- PANEL Y�NET�M� ---

    void ShowPanel(GameObject panelToShow)
    {
        Debug.Log($"[QuizGameManager] ShowPanel called: {(panelToShow != null ? panelToShow.name : "NULL (hide all)")}");
        
        if (startPanel) startPanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(false);
        if (inputPanel) inputPanel.SetActive(false);
        if (endPanel) endPanel.SetActive(false);

        if (panelToShow) 
        {
            panelToShow.SetActive(true);
            Debug.Log($"[QuizGameManager] Panel {panelToShow.name} is now ACTIVE");
        }
    }

    public void CloseQuiz()
    {
        ShowPanel(null); // Hepsini kapat
    }
}