using UnityEngine;

public class QuizBasket : MonoBehaviour
{
    [Header("Basket Settings")]
    public string acceptedArticle; // "Der", "Die" veya "Das"

    [Header("Lights for THIS Basket")]
    // Her sepet kendi tepesindeki ýþýðý bilecek
    public GameObject myGreenLight;
    public GameObject myRedLight;

    private void Start()
    {
        myGreenLight.SetActive(false);
        myRedLight.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ýçine giren objede WordItem scripti var mý?
        WordItem item = other.GetComponent<WordItem>();

        if (item == null)
            item = other.GetComponentInParent<WordItem>();

        if (item != null)
        {
            // Manager'a sorarken kendi ýþýklarýmýzý da gönderiyoruz
            // "Bu obje bana geldi, bu da benim ýþýklarým, kontrol et" diyoruz.
            QuizManager.Instance.CheckAnswer(item.objectID, acceptedArticle, myGreenLight, myRedLight);

            // Objeyi yok edebiliriz (Ýsteðe baðlý)
            // Destroy(other.gameObject, 0.5f); 
        }
    }
}