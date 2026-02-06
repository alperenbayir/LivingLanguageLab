using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Article sorting challenge - final game phase.
/// Player grabs kitchen objects and drops them into correct Der/Die/Das baskets.
/// Correct = green light + object deleted. Wrong = red light (try again).
/// Sorted items are tracked by ID in GameFlowController to exclude from future rounds.
/// </summary>
public class ArticleCleaningController : MonoBehaviour
{
    public static ArticleCleaningController Instance;

    [Header("Baskets")]
    public QuizBasket derBasket;
    public QuizBasket dieBasket;
    public QuizBasket dasBasket;

    [Header("Feedback Settings")]
    public float lightDuration = 1.5f;
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    [Header("Wrong Answer")]
    public Transform wrongAnswerSpawnPoint; // Objects teleport here on wrong answer

    // State
    private bool isCleaningMode = false;
    private List<WordItem> targetObjects = new List<WordItem>();
    private Dictionary<string, QuizBasket> articleToBasket = new Dictionary<string, QuizBasket>();
    private HashSet<WordItem> processingItems = new HashSet<WordItem>(); // Prevent duplicate processing

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Map articles to baskets
        if (derBasket != null) articleToBasket["DER"] = derBasket;
        if (dieBasket != null) articleToBasket["DIE"] = dieBasket;
        if (dasBasket != null) articleToBasket["DAS"] = dasBasket;

        // Auto-find baskets if not assigned
        if (derBasket == null || dieBasket == null || dasBasket == null)
        {
            FindBaskets();
        }
    }

    void FindBaskets()
    {
        QuizBasket[] allBaskets = FindObjectsByType<QuizBasket>(FindObjectsSortMode.None);
        foreach (var basket in allBaskets)
        {
            string article = basket.acceptedArticle?.ToUpper();
            if (article == "DER") derBasket = basket;
            else if (article == "DIE") dieBasket = basket;
            else if (article == "DAS") dasBasket = basket;
        }

        // Rebuild dictionary
        articleToBasket.Clear();
        if (derBasket != null) articleToBasket["DER"] = derBasket;
        if (dieBasket != null) articleToBasket["DIE"] = dieBasket;
        if (dasBasket != null) articleToBasket["DAS"] = dasBasket;
    }

    /// <summary>
    /// Called by GameFlowController to start cleaning mode
    /// </summary>
    public void StartCleaningMode(List<WordItem> objectsToClean)
    {
        isCleaningMode = true;
        targetObjects = new List<WordItem>(objectsToClean);

        // Enable baskets
        EnableBaskets(true);

        Debug.Log($"[ArticleCleaning] Started with {targetObjects.Count} objects to sort");
    }

    /// <summary>
    /// Called by QuizBasket when object enters
    /// </summary>
    public void OnObjectInBasket(WordItem item, string basketArticle)
    {
        if (!isCleaningMode) return;
        if (item == null) return;

        // Prevent duplicate processing (OnTriggerStay fires every frame)
        if (processingItems.Contains(item)) return;

        // Check if object is being held - only process when dropped
        var grabInteractable = item.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            // Object is still being held, don't process yet
            return;
        }

        // Check if this is one of our target objects
        if (!targetObjects.Contains(item))
        {
            return;
        }

        // Mark as processing
        processingItems.Add(item);

        // Get correct article from vocabulary
        ItemData data = VocabularyManager.Instance?.GetItem(item.objectID);
        if (data == null) return;

        string correctArticle = data.article_only?.Trim().ToUpper();
        string submittedArticle = basketArticle?.Trim().ToUpper();

        QuizBasket basket = GetBasketByArticle(submittedArticle);

        // Check answer
        if (correctArticle == submittedArticle)
        {
            Debug.Log($"[ArticleCleaning] CORRECT! {item.objectID} -> {submittedArticle}");
            StartCoroutine(CorrectAnswer(item, basket));
        }
        else
        {
            Debug.Log($"[ArticleCleaning] WRONG! {item.objectID} is {correctArticle}, not {submittedArticle}");
            StartCoroutine(WrongAnswer(basket, item));
        }
    }

    IEnumerator CorrectAnswer(WordItem item, QuizBasket basket)
    {
        // Show green light
        if (basket != null && basket.myGreenLight != null)
        {
            basket.myGreenLight.SetActive(true);
        }

        // Play correct sound
        if (audioSource != null && correctSound != null)
        {
            audioSource.PlayOneShot(correctSound);
        }

        // Remove from target list
        targetObjects.Remove(item);
        processingItems.Remove(item);

        // Delete the object
        if (item != null)
        {
            Destroy(item.gameObject);
        }

        // Notify GameFlowController
        GameFlowController.Instance?.OnObjectCleaned(item);

        // Wait then turn off light
        yield return new WaitForSeconds(lightDuration);

        if (basket != null && basket.myGreenLight != null)
        {
            basket.myGreenLight.SetActive(false);
        }
    }

    IEnumerator WrongAnswer(QuizBasket basket, WordItem item)
    {
        // Show red light
        if (basket != null && basket.myRedLight != null)
        {
            basket.myRedLight.SetActive(true);
        }

        // Play wrong sound
        if (audioSource != null && wrongSound != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }

        // Teleport object to retry spawn point
        if (item != null && wrongAnswerSpawnPoint != null)
        {
            item.transform.position = wrongAnswerSpawnPoint.position;
            Debug.Log($"[ArticleCleaning] {item.objectID} teleported to retry point");
        }

        // Allow retry
        processingItems.Remove(item);

        // Wait then turn off light
        yield return new WaitForSeconds(lightDuration);

        if (basket != null && basket.myRedLight != null)
        {
            basket.myRedLight.SetActive(false);
        }
    }

    QuizBasket GetBasketByArticle(string article)
    {
        articleToBasket.TryGetValue(article?.ToUpper(), out QuizBasket basket);
        return basket;
    }

    void EnableBaskets(bool enable)
    {
        // Parent (BasketSpawnArea) controls visibility
        // This just enables/disables the QuizBasket trigger detection
        if (derBasket != null) derBasket.enabled = enable;
        if (dieBasket != null) dieBasket.enabled = enable;
        if (dasBasket != null) dasBasket.enabled = enable;
    }

    public void StopCleaningMode()
    {
        isCleaningMode = false;
        targetObjects.Clear();
        EnableBaskets(false);
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
