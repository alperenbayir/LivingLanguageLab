using UnityEngine;

public class TabletMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject MainMenuLayout;
    public GameObject SubMenuLayout;

    [Header("Ses")]
    public AudioSource sfxSource;
    public AudioClip clickSound;

    public void OpenSelectionMenu()
    {
        PlaySound();
        if (MainMenuLayout != null) MainMenuLayout.SetActive(false);
        if (SubMenuLayout != null) SubMenuLayout.SetActive(true);
    }

    public void BackToMainMenu()
    {
        PlaySound();
        if (SubMenuLayout != null) SubMenuLayout.SetActive(false);
        if (MainMenuLayout != null) MainMenuLayout.SetActive(true);
    }

    public void QuitGame()
    {
        PlaySound();
        Debug.Log("Oyundan çýkýlýyor...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void PlaySound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}