using UnityEngine;
using TMPro; // Standard Unity Text Tool

public class TabletDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI germanLabel; // Drag your 'Text_German' object here
    public TextMeshProUGUI sentenceText;
    public TextMeshProUGUI progressText;

    [Header("Manager Connections")]
    public VocabularyManager vocabManager;
    public SentenceGenerator sentenceGenerator; // Reference for llm-sentence generator

    [HideInInspector]
    public bool isProcessing = false; // Lock for processing

    void Start()
    {
        // Initialize the counter on startup
        if (progressText != null && VocabularyManager.Instance != null)
        {
            progressText.text = $"0/{VocabularyManager.Instance.GetTotalCount()}";
        }
    }

    // This is called by the Right Hand Scanner
    public void UpdateDisplay(WordItem item)
    {
        if (isProcessing) return;

        // Lock
        isProcessing = true;

        if (germanLabel != null)
        {
            // Get the German word from the scanned item
            string textToShow = item.germanWord;

            // For debugging
            if (string.IsNullOrEmpty(textToShow))
            {
                textToShow = item.objectID;
            }

            // Update the screen
            germanLabel.text = textToShow;
        }
        else
        {
            Debug.LogError("Tablet Error");
            isProcessing = false;
        }

        if (sentenceText) sentenceText.text = "Generating the sentence...";

        if (sentenceGenerator != null)
        {
            // Small function (lambda) that runs when the AI finishes
            sentenceGenerator.RequestSentence(item, (result) =>
            {
                if (sentenceText) sentenceText.text = result;
                UpdateProgressUI();
                isProcessing = false;
            });
        }
        else
        {
            Debug.LogError("SentenceGenerator is not linked");
        }
    }

    // Ensure this method exists in your script to avoid errors
    void UpdateProgressUI()
    {
        if (progressText != null && VocabularyManager.Instance != null)
        {
            int found = SentenceHistoryManager.GetDiscoveredCount();
            int total = VocabularyManager.Instance.GetTotalCount();
            progressText.text = $"{found}/{total}";
        }
    }
}