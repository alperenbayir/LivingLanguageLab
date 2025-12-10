using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.VersionControl;
using UnityEngine.Networking;
using System.Text;

// JSON Data Classes
[System.Serializable]
public class WordData
{
    public string id;
    public string german;
    public string english;
    public string sentence;
    public string audioFile;
}

[System.Serializable]
public class WordList
{
    public WordData[] items;
}

public class SmartLearningPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panelRoot;
    public TextMeshProUGUI germanText;
    public TextMeshProUGUI englishText;
    public TextMeshProUGUI sentenceText;
    public TextMeshProUGUI resultText;

    [Header("Buttons")]
    public Button speakerButton;
    public Button micButton;
    public Button exitButton;

    [Header("Article Colors")]
    public Color derColor = Color.blue;
    public Color dieColor = Color.red;
    public Color dasColor = Color.green;
    public Color defaultColor = Color.white;

    [Header("Voice AI (YENI)")]
    public AppVoiceExperience voiceExperience; // Mikrofon (Dinleme)
    public TTSSpeaker ttsSpeaker;              // Hoparlör (Konusma)

    private Dictionary<string, WordData> dataDictionary = new Dictionary<string, WordData>();
    private WordData currentWord;

    [System.Serializable]
    private class OllamaResponse
    {
        public string response;
    }

    private string generatedSentence;

    void Start()
    {
        LoadJSON();

        if (panelRoot != null) panelRoot.SetActive(false);

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ClosePanel);
        }
    }

    public void ClosePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void LoadJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "words.json");
        if (File.Exists(path) || Application.isEditor)
        {
            try
            {
                string jsonString = File.ReadAllText(path);
                WordList loadedData = JsonUtility.FromJson<WordList>(jsonString);
                foreach (var word in loadedData.items)
                {
                    if (!dataDictionary.ContainsKey(word.id))
                        dataDictionary.Add(word.id, word);
                }
            }
            catch (System.Exception e) { Debug.LogWarning("JSON Error: " + e.Message); }
        }
    }

    public void ShowWord(string wordID)
    {
        if (dataDictionary.ContainsKey(wordID))
        {
            currentWord = dataDictionary[wordID];

            if (germanText)
            {
                germanText.text = currentWord.german;
                UpdateArticleColor(currentWord.german);
            }

            if (englishText) englishText.text = currentWord.english;
            if (sentenceText) sentenceText.text = "Generating sentence...";
            if (resultText) resultText.text = "";

            if (panelRoot) panelRoot.SetActive(true);
            StartCoroutine(GenerateSentence());

            // Butonlari Bagla
            if (speakerButton)
            {
                speakerButton.onClick.RemoveAllListeners();
                speakerButton.onClick.AddListener(PlayAudio);
            }
            if (micButton)
            {
                micButton.onClick.RemoveAllListeners();
                micButton.onClick.AddListener(StartMicTest);
            }
        }
    }

    void UpdateArticleColor(string text)
    {
        if (text.StartsWith("Der ")) germanText.color = derColor;
        else if (text.StartsWith("Die ")) germanText.color = dieColor;
        else if (text.StartsWith("Das ")) germanText.color = dasColor;
        else germanText.color = defaultColor;
    }

    // --- YAPAY ZEKA KONUSMA (TTS) ---
    void PlayAudio()
    {
        if (currentWord == null) return;
        string textToSpeak = currentWord.german;

        if (ttsSpeaker != null)
        {
            Debug.Log("Konuşuluyor: " + textToSpeak);

            // --- BURAYI DEGISTIR ---
            // İsim vermeden sadece metni gönder.
            // Wit.ai otomatik olarak varsayılan bir ses atayacaktır.
            ttsSpeaker.Speak(textToSpeak);
        }
        else
        {
            Debug.LogError("HATA: TTS Speaker atanmamış!");
        }
    }

    // --- SES TANIMA BASLATMA ---
    void StartMicTest()
    {
        if (voiceExperience != null)
        {
            if (resultText)
            {
                resultText.text = "Listening...";
                resultText.color = Color.yellow;
            }
            if (micButton) micButton.interactable = false;

            // Mikrofonu Ac
            voiceExperience.Activate();
        }
        else
        {
            Debug.LogError("Voice Experience atanmadi! Simulasyon calisiyor.");
            StartCoroutine(SimulateSpeechRecognition());
        }
    }

    IEnumerator GenerateSentence()
    {
        string url = "http://localhost:11434/api/generate";

        string jsonBody = $@"
        {{
        ""model"": ""llama3:latest"",
        ""prompt"": ""Schreibe einen einfachen Satz zu '{currentWord.german}' für Deutschlerner bestehend aus Subjekt, dann Prädikat, dann Objekt. Maximal 7 Wörter. Der Satz muss '{currentWord.german}'. NUR der Satz."",
        ""stream"": false
        }}
        ";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;

                // JSON -> Objekt
                OllamaResponse result = JsonUtility.FromJson<OllamaResponse>(json);

                // Nur das response-Feld verwenden
                generatedSentence = result.response;

                if (sentenceText != null)
                    sentenceText.text = generatedSentence;

                Debug.Log($"AI-generated sentence for {currentWord.id}: " + generatedSentence);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
}
