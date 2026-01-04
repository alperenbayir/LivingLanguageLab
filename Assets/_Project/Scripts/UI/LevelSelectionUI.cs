using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("Mekan Butonlarý")]
    public Button kitchenButton;
    public Button cafeButton;

    [Header("Renk Ayarlarý")]
    public Color normalColor = Color.white; // Seçili olmayan renk (Beyaz)
    public Color activeColor = Color.green; // Seçili olan renk (Yeþil)

    [Header("Seviye Seçimi")]
    public Slider levelSlider;
    public Text levelDisplayLabel;

    [Header("Baþlat")]
    public Button startButton;

    private string currentSelectedLocation;
    private string currentSelectedLevel;
    private string[] levelNames = { "A1", "A2", "B1", "B2", "C1" };

    void Start()
    {
        // Buton dinleyicileri
        kitchenButton.onClick.AddListener(() => OnLocationSelected("Kitchen"));
        cafeButton.onClick.AddListener(() => OnLocationSelected("Cafe"));

        // Slider dinleyicisi
        levelSlider.onValueChanged.AddListener(OnLevelChanged);

        // Start butonu dinleyicisi
        startButton.onClick.AddListener(OnStartClicked);

        // Varsayýlan açýlýþ ayarlarý
        OnLocationSelected("Kitchen");
        OnLevelChanged(levelSlider.value);
    }

    void OnLocationSelected(string location)
    {
        currentSelectedLocation = location;

        // --- RENK DEÐÝÞTÝRME MANTIÐI BURADA ---
        // Hangi butona basýldýysa onu 'activeColor' yap, diðerini 'normalColor' yap.
        // Bu sayede slider'a da týklasan renkler deðiþmez.

        if (location == "Kitchen")
        {
            SetButtonColor(kitchenButton, activeColor);
            SetButtonColor(cafeButton, normalColor);
        }
        else if (location == "Cafe")
        {
            SetButtonColor(kitchenButton, normalColor);
            SetButtonColor(cafeButton, activeColor);
        }

        Debug.Log("Seçilen Mekan: " + currentSelectedLocation);
    }

    // Yardýmcý fonksiyon: Butonun Image bileþenini bulup rengini deðiþtirir
    void SetButtonColor(Button btn, Color col)
    {
        Image btnImage = btn.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = col;
        }
    }

    void OnLevelChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        currentSelectedLevel = levelNames[index];

        if (levelDisplayLabel != null)
            levelDisplayLabel.text = currentSelectedLevel;
    }

    void OnStartClicked()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.SelectedLevel = currentSelectedLevel;
            GameSession.Instance.SelectedEnvironment = currentSelectedLocation;
        }

        Debug.Log($"Oyun Baþlýyor! Yer: {currentSelectedLocation}, Seviye: {currentSelectedLevel}");

        if (currentSelectedLocation == "Kitchen")
        {
            SceneManager.LoadScene("Kitchen");
        }
        else if (currentSelectedLocation == "Cafe")
        {
            SceneManager.LoadScene("Cafe");
        }
    }
}