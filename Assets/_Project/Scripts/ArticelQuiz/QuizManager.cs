using UnityEngine;
using System.Collections;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Settings")]
    public float feedbackDuration = 1.5f;
    
    void Awake()
    {
        Instance = this;
    }

    // Fonksiyon artýk Sepetten gelen Iþýk objelerini (targetGreen, targetRed) parametre olarak alýyor
    public void CheckAnswer(string objectID, string basketArticle, GameObject targetGreen, GameObject targetRed)
    {
        ItemData itemData = VocabularyManager.Instance.GetItem(objectID);

        if (itemData == null) return;

        // Karþýlaþtýrma
        bool isCorrect = itemData.article_only.Trim().ToUpper() == basketArticle.Trim().ToUpper();

        if (isCorrect)
        {
            Debug.Log("DOÐRU!");
            // Sadece sepetin gönderdiði YEÞÝL ýþýðý yak
            StartCoroutine(BlinkLight(targetGreen));
        }
        else
        {
            Debug.Log("YANLIÞ!");
            // Sadece sepetin gönderdiði KIRMIZI ýþýðý yak
            StartCoroutine(BlinkLight(targetRed));
        }
    }

    IEnumerator BlinkLight(GameObject lightObj)
    {
        if (lightObj != null)
        {
            lightObj.SetActive(true); // Iþýðý Yak
            yield return new WaitForSeconds(feedbackDuration); // Bekle
            lightObj.SetActive(false); // Iþýðý Söndür
        }
    }
}