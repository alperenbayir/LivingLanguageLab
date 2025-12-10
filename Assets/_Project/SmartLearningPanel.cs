using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.VersionControl;
using UnityEngine.Networking;
using System.Text;

// Classes to match JSON data
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
    public GameObject panelRoot; // The panel itself (open/close)
    public TextMeshProUGUI germanText;
    public TextMeshProUGUI englishText;
    public TextMeshProUGUI sentenceText;
    public TextMeshProUGUI resultText;
    public Button speakerButton;
    public Button micButton;
    public AudioSource audioSource;

    private Dictionary<string, WordData> dataDictionary = new Dictionary<string, WordData>();
    private WordData currentWord; // The currently selected word

    [System.Serializable]
    private class OllamaResponse
    {
        public string response;
    }

    private string generatedSentence;

    void Start()
    {
        LoadJSON();
        panelRoot.SetActive(false); // Hidden at start
    }

    // 1. LOAD JSON
    void LoadJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "words.json");
        if (File.Exists(path))
        {
            string jsonString = File.ReadAllText(path);
            WordList loadedData = JsonUtility.FromJson<WordList>(jsonString);
            foreach (var word in loadedData.items)
            {
                dataDictionary.Add(word.id, word);
            }
        }
        else { Debug.LogError("JSON file not found!"); }
    }

    // Updated ShowWord function
    public void ShowWord(string wordID)
    {
        // If the panel was about to close (timer running), cancel it!
        CancelInvoke("DeactivatePanel");

        if (dataDictionary.ContainsKey(wordID))
        {
            currentWord = dataDictionary[wordID];

            // Fill UI elements
            if (germanText) germanText.text = currentWord.german;
            if (englishText) englishText.text = currentWord.english;
            if (sentenceText) sentenceText.text = "Generating sentence...";
            if (resultText) resultText.text = "";

            if (panelRoot) panelRoot.SetActive(true);
            StartCoroutine(GenerateSentence());

            // Button listeners
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

    // Updated HidePanel function
    public void HidePanel()
    {
        // Do not close immediately! Wait 1 second.
        // During this time, the user can still reach the button.
        Invoke("DeactivatePanel", 4.0f);
    }

    // The actual function that closes the panel
    void DeactivatePanel()
    {
        if (panelRoot) panelRoot.SetActive(false);
    }

    // 3. PLAY AUDIO (Speaker)
    void PlayAudio()
    {
        if (currentWord == null) return;

        // 1. Get the audio file name from JSON (e.g. "apple_audio")
        string audioName = currentWord.audioFile;

        // 2. Load audio from Resources folder
        AudioClip clip = Resources.Load<AudioClip>(audioName);

        if (clip != null)
        {
            // 3. Play sound once
            audioSource.PlayOneShot(clip);
            Debug.Log("🔊 Playing audio: " + audioName);
        }
        else
        {
            Debug.LogError("❌ Audio file not found! Check 'Assets/Resources'. File name should be: " + audioName);
        }
    }

    // 4. MICROPHONE TEST (Simulation / Wizard of Oz)
    // Real speech API integration is risky for presentation, so we simulate it.
    void StartMicTest()
    {
        StartCoroutine(SimulateSpeechRecognition());
    }

    IEnumerator SimulateSpeechRecognition()
    {
        micButton.interactable = false;
        resultText.text = "🎤 Listening...";
        resultText.color = Color.yellow;

        yield return new WaitForSeconds(2.0f); // Fake listening for 2 seconds

        // Generate a random accuracy value (in presentation, it will often be high)
        int accuracy = Random.Range(75, 100);

        if (accuracy > 80)
        {
            resultText.text = $"✅ Correct! ({accuracy}%)";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = $"❌ Retry. ({accuracy}%)";
            resultText.color = Color.red;
        }
        micButton.interactable = true;
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
