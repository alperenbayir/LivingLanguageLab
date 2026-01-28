using UnityEngine;

public class QuizBasket : MonoBehaviour
{
    [Header("Basket Settings")]
    public string acceptedArticle; // "Der", "Die" veya "Das"

    [Header("Lights for THIS Basket")]
    // Her sepet kendi tepesindeki ����� bilecek
    public GameObject myGreenLight;
    public GameObject myRedLight;

    private void Start()
    {
        myGreenLight.SetActive(false);
        myRedLight.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ��ine giren objede WordItem scripti var m�?
        WordItem item = other.GetComponent<WordItem>();

        if (item == null)
            item = other.GetComponentInParent<WordItem>();

        if (item != null)
        {
            // Check if we're in cleaning mode (ArticleCleaningController handles it)
            if (ArticleCleaningController.Instance != null && 
                ArticleCleaningController.Instance.IsCleaningMode())
            {
                // Route to ArticleCleaningController
                ArticleCleaningController.Instance.OnObjectInToilet(item, acceptedArticle);
                return;
            }

            // Manager'a sorarken kendi ���klar�m�z� da g�nderiyoruz
            // "Bu obje bana geldi, bu da benim ���klar�m, kontrol et" diyoruz.
            QuizManager.Instance.CheckAnswer(item.objectID, acceptedArticle, myGreenLight, myRedLight);

            // Objeyi yok edebiliriz (�ste�e ba�l�)
            // Destroy(other.gameObject, 0.5f); 
        }
    }
}