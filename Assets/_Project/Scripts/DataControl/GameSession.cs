using UnityEngine;

public class GameSession : MonoBehaviour
{
    // Bu koda her yerden ulaþabilmek için Singleton yapýsý kuruyoruz
    public static GameSession Instance;

    // Kaydedeceðimiz veriler
    public string SelectedLevel;       // A1, B2 vb.
    public string SelectedEnvironment; // Kitchen, Cafe

    void Awake()
    {
        // Eðer daha önce oluþmuþ bir GameSession varsa, kendini yok et (kopya olmasýn)
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Ýlk kez oluþuyorsa, bu objeyi ben yönetiyorum de
        Instance = this;
        // Sahne deðiþince bu objeyi yok etme!
        DontDestroyOnLoad(gameObject);
    }
}