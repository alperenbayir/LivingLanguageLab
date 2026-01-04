using UnityEngine;

public class TabletMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject MainMenuLayout;      // Þu anki Play/Exit butonlarýnýn olduðu panel
    public GameObject SubMenuLayout; // Yeni açýlacak seçenekler paneli

    // Play butonuna basýnca çalýþacak fonksiyon
    public void OpenSelectionMenu()
    {
        MainMenuLayout.SetActive(false);      // Ana menüyü gizle
        SubMenuLayout.SetActive(true);  // Seçenekler menüsünü aç
    }

    // (Ýsteðe baðlý) Geri dönmek istersen diye
    public void BackToMainMenu()
    {
        MainMenuLayout.SetActive(false); // Seçenekleri gizle
        SubMenuLayout.SetActive(true);       // Ana menüyü geri aç
    }

    // Exit butonu için
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Oyundan çýkýldý."); // Editörde çalýþtýðýný görmek için
    }
}