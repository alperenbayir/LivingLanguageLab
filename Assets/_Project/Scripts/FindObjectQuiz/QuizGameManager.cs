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
        // Only show start panel if not using external challenge trigger
        if (!isExternalChallenge)
        {
            ShowPanel(startPanel);
        }
    }
    
    /// <summary>
    /// Called by GameFlowController to start challenge from exploration
    /// </summary>
    public void StartExternalChallenge(List<string> itemIDs, bool timed)
    {
        isExternalChallenge = true;
        externalItemIDs = new List<string>(itemIDs);
        challengeTimed = timed;
        
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
            AskNextQuestion();
        }
        else
        {
            currentTime += 5f;
            StartCoroutine(ShowFeedback("Wrong! (+5 sec)", penaltyColor));
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
        
        // If external challenge, notify GameFlowController instead of showing leaderboard
        if (isExternalChallenge)
        {
            GameFlowController.Instance?.OnFindChallengeComplete(currentTime);
            isExternalChallenge = false;
            ShowPanel(null); // Hide all panels
        }
        else
        {
            ShowPanel(inputPanel);
        }
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

    public void SubmitScore()
    {
        string playerName = nameInput.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";

        LeaderboardManager.Instance.SaveScore(playerName, currentTime);
        UpdateLeaderboardDisplay();
        ShowPanel(endPanel);
    }

    public void UpdateLeaderboardDisplay()
    {
        // 1. �nceki listeyi temizle
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        LeaderboardData data = LeaderboardManager.Instance.LoadScores();

        // 2. Yeni listeyi olu�tur
        for (int i = 0; i < data.scores.Count; i++)
        {
            GameObject row = Instantiate(scoreRowPrefab, leaderboardContainer);

            // D�KKAT: Burada i de�erini fonksiyona parametre olarak at�yoruz.
            // Bu sayede Closure (de�i�ken kar��ma) problemi imkans�z hale geliyor.
            SetupScoreRow(row, i, data.scores[i]);
        }
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
        LeaderboardManager.Instance.ClearAllScores();
        UpdateLeaderboardDisplay();
    }

    // --- PANEL Y�NET�M� ---

    void ShowPanel(GameObject panelToShow)
    {
        if (startPanel) startPanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(false);
        if (inputPanel) inputPanel.SetActive(false);
        if (endPanel) endPanel.SetActive(false);

        if (panelToShow) panelToShow.SetActive(true);
    }

    public void CloseQuiz()
    {
        ShowPanel(null); // Hepsini kapat
    }
}