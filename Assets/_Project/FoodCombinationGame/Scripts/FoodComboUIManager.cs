using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class FoodComboUIManager : MonoBehaviour
{
    public static FoodComboUIManager Instance;

    [Header("UI")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI countdownText;

    private const string StartPrompt = "Combine ingredients to cook dishes and expand your German vocabulary!";
    private const string ScanPrompt = "Scan the new dish to learn it in German!";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowPrompt(StartPrompt);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    public void ShowScanPrompt()
    {
        ShowPrompt(ScanPrompt);
    }

    public void OnPizzaCrafted()
    {
        ShowScanPrompt();
    }

    public void OnPizzaScanned()
    {
        StartCoroutine(PrepositionCountdown());
    }

    private IEnumerator PrepositionCountdown()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        for (int i = 5; i >= 1; i--)
        {
            if (countdownText != null)
                countdownText.text = $"Preposition Game unlocked! Starting in {i}..";
            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene("Preposition");
    }

    public void ShowPrompt(string message)
    {
        if (promptText != null)
            promptText.text = message;
    }
}
