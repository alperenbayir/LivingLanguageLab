using UnityEngine;

public class TabletMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject MainMenuLayout;  // Play ve Exit butonlarýnýn olduðu ana panel
    public GameObject SubMenuLayout;   // Resimlerin ve Level Slider'ýn olduðu panel

    // Play butonuna basýnca: Ana menüyü kapat, Seçenekleri aç
    public void OpenSelectionMenu()
    {
        if (MainMenuLayout != null) MainMenuLayout.SetActive(false);
        if (SubMenuLayout != null) SubMenuLayout.SetActive(true);
    }

    // Geri butonuna basýnca: Seçenekleri kapat, Ana menüyü aç
    public void BackToMainMenu()
    {
        // (Burada ufak bir düzeltme yaptým: Geri dönmek için Sub kapanmalý, Main açýlmalý)
        if (SubMenuLayout != null) SubMenuLayout.SetActive(false);
        if (MainMenuLayout != null) MainMenuLayout.SetActive(true);
    }

    // Exit butonuna basýnca çalýþacak fonksiyon
    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");

#if UNITY_EDITOR
        // Eðer Unity Editöründeysek 'Play' modunu durdurur
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Eðer oyun Build alýnmýþsa (telefonda/PC'de) uygulamayý tamamen kapatýr
            Application.Quit();
#endif
    }
}