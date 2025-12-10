using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
    public Color derColor = Color.blue;   // Masculine
    public Color dieColor = Color.red;    // Feminine
    public Color dasColor = Color.green;  // Neuter
    public Color defaultColor = Color.white; // Standard

    public AudioSource audioSource;

    private Dictionary<string, WordData> dataDictionary = new Dictionary<string, WordData>();
    private WordData currentWord;

    void Start()
    {
        LoadJSON();

        // Hide panel at start
        if (panelRoot != null) panelRoot.SetActive(false);

        // Setup Exit Button
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
        // Check if file exists (or if running in Editor)
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

            // Populate UI
            if (germanText)
            {
                germanText.text = currentWord.german;
                // Update color based on article
                UpdateArticleColor(currentWord.german);
            }

            if (englishText) englishText.text = currentWord.english;
            if (sentenceText) sentenceText.text = currentWord.sentence;
            if (resultText) resultText.text = ""; // Clear previous result

            if (panelRoot) panelRoot.SetActive(true);

            // Setup Listeners
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

    // Color Update Logic
    void UpdateArticleColor(string text)
    {
        // If text starts with "Der " -> Blue
        if (text.StartsWith("Der "))
        {
            germanText.color = derColor;
        }
        // If text starts with "Die " -> Red
        else if (text.StartsWith("Die "))
        {
            germanText.color = dieColor;
        }
        // If text starts with "Das " -> Green
        else if (text.StartsWith("Das "))
        {
            germanText.color = dasColor;
        }
        // Otherwise -> White
        else
        {
            germanText.color = defaultColor;
        }
    }

    void PlayAudio()
    {
        if (currentWord == null) return;

        string audioName = currentWord.audioFile;
        // Load from Resources folder
        AudioClip clip = Resources.Load<AudioClip>(audioName);

        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log("Playing audio: " + audioName);
        }
        else
        {
            Debug.LogError("Audio file not found! Please check 'Assets/Resources'. File name should be: " + audioName);
        }
    }

    void StartMicTest()
    {
        StartCoroutine(SimulateSpeechRecognition());
    }

    // Wizard of Oz Simulation
    IEnumerator SimulateSpeechRecognition()
    {
        if (micButton) micButton.interactable = false;

        if (resultText)
        {
            resultText.text = "Listening...";
            resultText.color = Color.yellow;
        }

        yield return new WaitForSeconds(2.0f); // Wait for 2 seconds

        int accuracy = Random.Range(85, 100); // Random high accuracy

        if (resultText)
        {
            if (accuracy > 80)
            {
                resultText.text = $"Correct! ({accuracy}%)";
                resultText.color = Color.green;
            }
            else
            {
                resultText.text = $"Try again. ({accuracy}%)";
                resultText.color = Color.red;
            }
        }

        if (micButton) micButton.interactable = true;
    }
}