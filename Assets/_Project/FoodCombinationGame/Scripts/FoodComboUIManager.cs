using UnityEngine;
using TMPro;

public class FoodComboUIManager : MonoBehaviour
{
    public static FoodComboUIManager Instance;

    [Header("UI")]
    public TextMeshProUGUI promptText;

    private const string StartPrompt = "Combine ingredients to cook dishes and expand your German vocabulary!";
    private const string ScanPrompt = "Scan the new dish to learn it in German!";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowPrompt(StartPrompt);
    }

    public void ShowScanPrompt()
    {
        ShowPrompt(ScanPrompt);
    }

    public void ShowPrompt(string message)
    {
        if (promptText != null)
            promptText.text = message;
    }
}
