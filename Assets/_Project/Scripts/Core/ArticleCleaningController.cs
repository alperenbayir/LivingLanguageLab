using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Viscera Cleanup Detail style cleaning game.
/// Player grabs actual kitchen objects and throws them into correct article toilets.
/// </summary>
public class ArticleCleaningController : MonoBehaviour
{
    public static ArticleCleaningController Instance;

    [Header("Toilets")]
    public QuizBasket derToilet;
    public QuizBasket dieToilet;
    public QuizBasket dasToilet;

    [Header("Feedback")]
    public float flushDelay = 0.5f;
    public ParticleSystem flushParticlesPrefab;

    // State
    private bool isCleaningMode = false;
    private List<WordItem> targetObjects = new List<WordItem>();
    private Dictionary<string, QuizBasket> articleToToilet = new Dictionary<string, QuizBasket>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Map articles to toilets
        if (derToilet != null) articleToToilet["DER"] = derToilet;
        if (dieToilet != null) articleToToilet["DIE"] = dieToilet;
        if (dasToilet != null) articleToToilet["DAS"] = dasToilet;

        // Auto-find toilets if not assigned
        if (derToilet == null || dieToilet == null || dasToilet == null)
        {
            FindToilets();
        }
    }

    void FindToilets()
    {
        QuizBasket[] allBaskets = FindObjectsOfType<QuizBasket>();
        foreach (var basket in allBaskets)
        {
            string article = basket.acceptedArticle?.ToUpper();
            if (article == "DER") derToilet = basket;
            else if (article == "DIE") dieToilet = basket;
            else if (article == "DAS") dasToilet = basket;
        }

        // Rebuild dictionary
        articleToToilet.Clear();
        if (derToilet != null) articleToToilet["DER"] = derToilet;
        if (dieToilet != null) articleToToilet["DIE"] = dieToilet;
        if (dasToilet != null) articleToToilet["DAS"] = dasToilet;
    }

    /// <summary>
    /// Called by GameFlowController to start cleaning mode
    /// </summary>
    public void StartCleaningMode(List<WordItem> objectsToClean)
    {
        isCleaningMode = true;
        targetObjects = new List<WordItem>(objectsToClean);

        // Enable collision detection on toilets for these objects
        EnableToilets(true);

        Debug.Log($"[ArticleCleaning] Started with {targetObjects.Count} objects to clean");
    }

    /// <summary>
    /// Called when object enters any toilet basket
    /// </summary>
    public void OnObjectInToilet(WordItem item, string toiletArticle)
    {
        if (!isCleaningMode) return;
        if (item == null) return;

        // Check if this is one of our target objects
        if (!targetObjects.Contains(item))
        {
            Debug.Log($"[ArticleCleaning] Object {item.objectID} is not part of cleaning task");
            return;
        }

        // Get correct article from vocabulary
        ItemData data = VocabularyManager.Instance?.GetItem(item.objectID);
        if (data == null) return;

        string correctArticle = data.article_only?.Trim().ToUpper();
        string submittedArticle = toiletArticle?.Trim().ToUpper();

        // Check answer
        if (correctArticle == submittedArticle)
        {
            Debug.Log($"[ArticleCleaning] CORRECT! {item.objectID} -> {submittedArticle}");
            StartCoroutine(FlushObject(item, toiletArticle));
        }
        else
        {
            Debug.Log($"[ArticleCleaning] WRONG! {item.objectID} is {correctArticle}, not {submittedArticle}");
            // Wrong toilet - maybe bounce back or show red light
            WrongToiletFeedback(item, toiletArticle);
        }
    }

    System.Collections.IEnumerator FlushObject(WordItem item, string toiletArticle)
    {
        // Play particles at toilet position
        QuizBasket toilet = GetToiletByArticle(toiletArticle);
        if (toilet != null && flushParticlesPrefab != null)
        {
            Vector3 particlePos = toilet.transform.position + Vector3.up * 0.5f;
            Instantiate(flushParticlesPrefab, particlePos, Quaternion.identity);
        }

        // Wait for flush sound
        yield return new WaitForSeconds(flushDelay);

        // Notify GameFlowController
        GameFlowController.Instance?.OnObjectCleaned(item);

        // Remove from our list
        targetObjects.Remove(item);
    }

    void WrongToiletFeedback(WordItem item, string toiletArticle)
    {
        // Get the toilet's red light and blink it
        QuizBasket toilet = GetToiletByArticle(toiletArticle);
        if (toilet != null)
        {
            // You could add a bounce-back mechanism here
            // For now, just let it sit there for player to pick up again
            Debug.Log($"[ArticleCleaning] Wrong toilet feedback for {toiletArticle}");
        }
    }

    QuizBasket GetToiletByArticle(string article)
    {
        articleToToilet.TryGetValue(article?.ToUpper(), out QuizBasket toilet);
        return toilet;
    }

    void EnableToilets(bool enable)
    {
        if (derToilet != null) derToilet.enabled = enable;
        if (dieToilet != null) dieToilet.enabled = enable;
        if (dasToilet != null) dasToilet.enabled = enable;
    }

    public void StopCleaningMode()
    {
        isCleaningMode = false;
        targetObjects.Clear();
        EnableToilets(false);
    }

    public bool IsCleaningMode()
    {
        return isCleaningMode;
    }

    public int GetRemainingCount()
    {
        return targetObjects.Count;
    }
}
