using UnityEngine;
using TMPro; // Standard Unity Text Tool

public class TabletDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI germanLabel;// Drag your 'Text_German' object here
    public TextMeshProUGUI sentenceText;
    public SentenceGenerator sentenceGenerator; // reference for llm-sentence generator


    [HideInInspector]
    public bool isProcessing = false; //lock for processing

    // This is called by the Right Hand Scanner
    public void UpdateDisplay(WordItem item)
    {

        // 
        if (isProcessing) return;

        // Lock
        isProcessing = true;


        if (germanLabel != null)
        {
            // Get the German word from the scanned item
            string textToShow = item.germanWord;

            // for debugging
            if (string.IsNullOrEmpty(textToShow))
            {
                textToShow = item.objectID;
            }

            //  Update the screen
            germanLabel.text = textToShow;
        }
        else
        {
            Debug.LogError("Tablet Error");
            isProcessing = false;
        }

        if (sentenceText) sentenceText.text = "Generiere...";

        if (sentenceGenerator != null)
        {
           // small function (lambda) that runs when the AI finishes
            sentenceGenerator.RequestSentence(item, (result) =>
            {
                if (sentenceText) sentenceText.text = result;
                isProcessing = false;
            });
        }
        else
        {
            Debug.LogError("SentenceGenerator is not linked");
        }
    }
}