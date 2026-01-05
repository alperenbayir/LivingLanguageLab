using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // Coroutine için gerekli
using TMPro; 

public class LevelSelectionUI : MonoBehaviour
{
    [Header("Mekan Butonlarý")]
    public Button kitchenButton;
    public Button cafeButton;

    [Header("Renk Ayarlarý")]
    public Color normalColor = Color.white;
    public Color activeColor = Color.green;

    [Header("Seviye Seçimi")]
    public Slider levelSlider;
    public Text levelDisplayLabel;

    [Header("Baþlat")]
    public Button startButton;

    [Header("Ses Ayarlarý")]
    public AudioSource sfxSource;    // Efekt seslerini çalacak kaynak
    public AudioClip clickSound;     // Týklama sesi dosyasý

    [Header("Teleport Animasyonu")]
    public GameObject subPanel;
    public GameObject loadingPanel;  // "Teleporting..." yazan siyah ekran paneli
    public TextMeshProUGUI loadingText; // Geri sayým yazýsý (3.. 2.. 1..)

    private string currentSelectedLocation;
    private string currentSelectedLevel;
    private string[] levelNames = { "A1", "A2", "B1", "B2", "C1" };

    void Start()
    {
        // Loading panelini baþta gizle
        if (loadingPanel != null) loadingPanel.SetActive(false);

        // Buton dinleyicileri
        kitchenButton.onClick.AddListener(() => {
            PlayClickSound();
            OnLocationSelected("Kitchen");
        });

        cafeButton.onClick.AddListener(() => {
            PlayClickSound();
            OnLocationSelected("Cafe");
        });

        levelSlider.onValueChanged.AddListener(OnLevelChanged);

        // Start butonu artýk Coroutine baþlatýyor
        startButton.onClick.AddListener(OnStartClicked);

        // Varsayýlan ayarlar
        OnLocationSelected("Kitchen");
        OnLevelChanged(levelSlider.value);
    }

    void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    void OnLocationSelected(string location)
    {
        currentSelectedLocation = location;

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
    }

    void SetButtonColor(Button btn, Color col)
    {
        Image btnImage = btn.GetComponent<Image>();
        if (btnImage != null) btnImage.color = col;
    }

    void OnLevelChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        currentSelectedLevel = levelNames[index];
        if (levelDisplayLabel != null) levelDisplayLabel.text = currentSelectedLevel;
    }

    // --- BURASI DEÐÝÞTÝ: GERÝ SAYIM BAÞLATIYOR ---
    void OnStartClicked()
    {
        PlayClickSound(); // Ses çal
        StartCoroutine(TeleportSequence());
    }

    IEnumerator TeleportSequence()
    {
        // 1. Verileri Kaydet
        if (GameSession.Instance != null)
        {
            GameSession.Instance.SelectedLevel = currentSelectedLevel;
            GameSession.Instance.SelectedEnvironment = currentSelectedLocation;
        }

        // 2. Paneli Aç
        if (loadingPanel != null)
        {
            subPanel.SetActive(false);
            loadingPanel.SetActive(true);

            // Geri Sayým
            if (loadingText != null) loadingText.text = "Teleporting...  3";
            yield return new WaitForSeconds(1f);

            if (loadingText != null) loadingText.text = "Teleporting...  2";
            yield return new WaitForSeconds(1f);

            if (loadingText != null) loadingText.text = "Teleporting...  1";
            yield return new WaitForSeconds(1f);
        }
        else
        {
            // Panel yoksa beklemeden geçmesin diye minik bir bekleme
            yield return new WaitForSeconds(0.5f);
        }

        // 3. Sahneyi Yükle
        Debug.Log($"Yükleniyor: {currentSelectedLocation}");

        if (currentSelectedLocation == "Kitchen")
            SceneManager.LoadScene("Kitchen");
        else if (currentSelectedLocation == "Cafe")
            SceneManager.LoadScene("Cafe");
    }
}