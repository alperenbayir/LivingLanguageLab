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
        if (myGreenLight != null) myGreenLight.SetActive(false);
        if (myRedLight != null) myRedLight.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        WordItem item = other.GetComponent<WordItem>();

        if (item == null)
            item = other.GetComponentInParent<WordItem>();

        if (item != null && ArticleCleaningController.Instance != null)
        {
            ArticleCleaningController.Instance.OnObjectInBasket(item, acceptedArticle);
        }
    }
}