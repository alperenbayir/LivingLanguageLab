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
    public Transform leaderboardContainer; // Satýrlarýn dizileceði kutu
    public GameObject scoreRowPrefab;      // Satýr þablonu

    [Header("Game Settings")]
    public int totalQuestions = 5;
    public Color correctColor = Color.green;
    public Color penaltyColor = Color.red;

    public bool IsGameActive { get; private set; } = false;
    private float currentTime = 0f;
    private List<ItemData> questionQueue = new List<ItemData>();
    private ItemData currentTarget;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowPanel(startPanel);
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

    // --- OYUN AKIÞI ---

    public void StartGame()
    {
        currentTime = 0f;
        PrepareQuestions();
        IsGameActive = true;
        ShowPanel(gamePanel);
        AskNextQuestion();
    }

    void PrepareQuestions()
    {
        if (VocabularyManager.Instance == null) return;

        List<ItemData> allItems = new List<ItemData>(VocabularyManager.Instance.database.Values);

        // Karýþtýr
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
            feedbackText.text = "";
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
        ShowPanel(inputPanel);
    }

    public void RestartGame()
    {
        ShowPanel(startPanel);
    }

    // --- SKOR VE LÝDERLÝK TABLOSU ---

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
        // 1. Önceki listeyi temizle
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        LeaderboardData data = LeaderboardManager.Instance.LoadScores();

        // 2. Yeni listeyi oluþtur
        for (int i = 0; i < data.scores.Count; i++)
        {
            GameObject row = Instantiate(scoreRowPrefab, leaderboardContainer);

            // DÝKKAT: Burada i deðerini fonksiyona parametre olarak atýyoruz.
            // Bu sayede Closure (deðiþken karýþma) problemi imkansýz hale geliyor.
            SetupScoreRow(row, i, data.scores[i]);
        }
    }

    // Yardýmcý fonksiyon: Her satýrý ve butonu izole eder
    void SetupScoreRow(GameObject rowObj, int index, ScoreEntry entry)
    {
        // Yazýyý Ayarla
        TextMeshProUGUI[] texts = rowObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 1)
        {
            texts[0].text = $"{index + 1}. {entry.playerName} - {entry.timeScore:F1} sec";
        }

        // Butonu Ayarla
        Button deleteBtn = rowObj.GetComponentInChildren<Button>();
        if (deleteBtn != null)
        {
            // Eski baðlantýlarý temizle
            deleteBtn.onClick.RemoveAllListeners();

            // Yeni silme emrini ver (index deðeri burada sabitlenmiþtir)
            deleteBtn.onClick.AddListener(() =>
            {
                Debug.Log($"[QuizManager] Silme butonuna basýldý. Silinecek index: {index}");
                DeleteSingleScore(index-1);
            });
        }
    }

    public void DeleteSingleScore(int index)
    {
        Debug.Log($"[QuizManager] DeleteSingleScore çalýþtý. Index: {index}");
        LeaderboardManager.Instance.DeleteScoreAtIndex(index);
        UpdateLeaderboardDisplay(); // Listeyi yenile
    }

    public void ClearAllLeaderboard()
    {
        LeaderboardManager.Instance.ClearAllScores();
        UpdateLeaderboardDisplay();
    }

    // --- PANEL YÖNETÝMÝ ---

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