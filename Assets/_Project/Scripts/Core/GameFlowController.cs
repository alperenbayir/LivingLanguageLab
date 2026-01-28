using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Orchestrates the 3-phase Kitchen experience:
/// 1. Exploration (0-50%) - Free scanning
/// 2. Find Challenge (50%) - Timer-based hunt, non-destructive
/// 3. Cleaning Finale (90%) - Viscera-style article sorting with actual objects
/// </summary>
public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance;

    [Header("Phase Settings")]
    [Range(0.3f, 0.6f)]
    public float findChallengeThreshold = 0.5f; // 50%
    [Range(0.7f, 0.95f)]
    public float cleaningThreshold = 0.9f; // 90%

    [Header("FindObject Challenge Settings")]
    public int findChallengeObjectCount = 5;
    public bool findChallengeTimed = true;

    [Header("Cleaning Challenge Settings")]
    public int cleaningObjectCount = 10;
    public Transform toiletsParent; // Parent containing Der/Die/Das toilets
    public Transform cleaningSpawnArea; // Area to gather objects initially

    [Header("Audio Placeholders")]
    public AudioSource ambientAudioSource;
    public AudioClip challengeMusicClip;
    public AudioClip flushSoundClip;

    [Header("References")]
    public TabletDisplay tabletDisplay;
    public QuizGameManager quizGameManager;
    public RightHandScanner rightHandScanner;

    // Internal state
    private bool findChallengeOffered = false;
    private bool findChallengeCompleted = false;
    private bool cleaningChallengeStarted = false;
    private GamePhase currentPhase = GamePhase.Exploration;
    private List<WordItem> discoveredItems = new List<WordItem>();
    private List<WordItem> cleaningObjects = new List<WordItem>();
    private int cleanedCount = 0;

    public enum GamePhase
    {
        Exploration,
        FindChallengeActive,
        CleaningChallengeActive,
        Complete
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Validate references
        if (tabletDisplay == null)
            tabletDisplay = FindObjectOfType<TabletDisplay>();
        if (quizGameManager == null)
            quizGameManager = FindObjectOfType<QuizGameManager>();
        if (rightHandScanner == null)
            rightHandScanner = FindObjectOfType<RightHandScanner>();

        currentPhase = GamePhase.Exploration;
    }

    /// <summary>
    /// Called by TabletDisplay when new object is discovered
    /// </summary>
    public void OnObjectDiscovered(string objectID)
    {
        if (currentPhase == GamePhase.CleaningChallengeActive) return;

        float progress = GetDiscoveryProgress();

        // Check for Find Challenge trigger (50%)
        if (!findChallengeOffered && progress >= findChallengeThreshold)
        {
            findChallengeOffered = true;
            OfferFindChallenge();
        }
        // Check for Cleaning Challenge trigger (90%)
        else if (!cleaningChallengeStarted && progress >= cleaningThreshold && findChallengeCompleted)
        {
            OfferCleaningChallenge();
        }
    }

    float GetDiscoveryProgress()
    {
        if (VocabularyManager.Instance == null) return 0f;
        float discovered = SentenceHistoryManager.GetDiscoveredCount();
        float total = VocabularyManager.Instance.GetTotalCount();
        return discovered / total;
    }

    #region FIND OBJECT CHALLENGE (50%)

    void OfferFindChallenge()
    {
        Debug.Log("[GameFlow] Offering Find Challenge at 50%");
        
        // Dim lights and start music
        StartChallengeAtmosphere();

        // Show offer on tablet
        tabletDisplay.ShowFindChallengePrompt(
            onAccept: StartFindChallenge,
            onDecline: () => {
                Debug.Log("[GameFlow] Find Challenge declined");
                // Will re-offer on next discovery
                findChallengeOffered = false;
            }
        );
    }

    void StartFindChallenge()
    {
        Debug.Log("[GameFlow] Starting Find Challenge");
        currentPhase = GamePhase.FindChallengeActive;

        // Get discovered items
        List<string> discoveredIDs = GetDiscoveredItemIDs();
        
        // Prepare challenge with 5 random discovered items
        List<string> challengeItems = GetRandomItems(discoveredIDs, findChallengeObjectCount);
        
        // Start the quiz with external trigger
        quizGameManager.StartExternalChallenge(challengeItems, findChallengeTimed);
    }

    public void OnFindChallengeComplete(float finalTime)
    {
        Debug.Log($"[GameFlow] Find Challenge complete! Time: {finalTime:F1}s");
        findChallengeCompleted = true;
        currentPhase = GamePhase.Exploration;

        // Restore atmosphere
        EndChallengeAtmosphere();

        // Player can continue exploring
        tabletDisplay.ShowMessage("Challenge complete! Continue exploring...");
    }

    public void OnFindChallengeCancelled()
    {
        Debug.Log("[GameFlow] Find Challenge cancelled");
        currentPhase = GamePhase.Exploration;
        EndChallengeAtmosphere();
        
        // Re-offer later
        findChallengeOffered = false;
    }

    #endregion

    #region CLEANING CHALLENGE (90%)

    void OfferCleaningChallenge()
    {
        Debug.Log("[GameFlow] Offering Cleaning Challenge at 90%");
        
        // Dramatic atmosphere
        StartChallengeAtmosphere();

        // Auto-start or show big prompt
        tabletDisplay.ShowCleaningChallengePrompt(
            onAccept: StartCleaningChallenge,
            onDecline: () => {
                // Can continue to 100%, offer again later
                Debug.Log("[GameFlow] Cleaning Challenge delayed");
            }
        );
    }

    void StartCleaningChallenge()
    {
        Debug.Log("[GameFlow] Starting Cleaning Challenge");
        currentPhase = GamePhase.CleaningChallengeActive;
        cleaningChallengeStarted = true;
        cleanedCount = 0;

        // Collect actual kitchen objects for cleaning
        cleaningObjects = GetKitchenObjectsForCleaning(cleaningObjectCount);
        
        // Gather them to cleaning area
        GatherObjectsForCleaning();

        // Setup tablet for cleaning UI
        tabletDisplay.EnterCleaningMode(cleaningObjects.Count);

        // Enable cleaning interaction
        ArticleCleaningController.Instance?.StartCleaningMode(cleaningObjects);

        UpdateCleaningUI();
    }

    List<WordItem> GetKitchenObjectsForCleaning(int count)
    {
        // Get all WordItems in scene
        WordItem[] allItems = FindObjectsOfType<WordItem>();
        
        // Filter to discovered ones first, then add undiscovered if needed
        List<WordItem> discovered = allItems.Where(w => 
            SentenceHistoryManager.IsDiscovered(w.objectID)).ToList();
        
        List<WordItem> selected = new List<WordItem>();
        
        // Add discovered items first
        selected.AddRange(discovered.OrderBy(x => Random.value).Take(Mathf.Min(count, discovered.Count)));
        
        // Fill with undiscovered if needed
        if (selected.Count < count)
        {
            var undiscovered = allItems.Where(w => !SentenceHistoryManager.IsDiscovered(w.objectID))
                .OrderBy(x => Random.value).Take(count - selected.Count);
            selected.AddRange(undiscovered);
        }
        
        return selected.Take(count).ToList();
    }

    void GatherObjectsForCleaning()
    {
        // Move objects to cleaning spawn area
        if (cleaningSpawnArea != null)
        {
            Vector3 center = cleaningSpawnArea.position;
            for (int i = 0; i < cleaningObjects.Count; i++)
            {
                if (cleaningObjects[i] != null)
                {
                    // Random position around center
                    Vector3 offset = new Vector3(
                        Random.Range(-0.5f, 0.5f),
                        0.1f,
                        Random.Range(-0.5f, 0.5f)
                    );
                    cleaningObjects[i].transform.position = center + offset;
                    
                    // Make sure they're grabbable
                    EnableGrabbing(cleaningObjects[i], true);
                }
            }
        }
    }

    public void OnObjectCleaned(WordItem item)
    {
        cleanedCount++;
        UpdateCleaningUI();

        // Play flush sound
        if (flushSoundClip != null && ambientAudioSource != null)
        {
            ambientAudioSource.PlayOneShot(flushSoundClip);
        }

        // Disable the object (it's "flushed")
        if (item != null)
        {
            EnableGrabbing(item, false);
            item.gameObject.SetActive(false); // Or destroy: Destroy(item.gameObject);
        }

        // Check completion
        if (cleanedCount >= cleaningObjects.Count)
        {
            OnCleaningComplete();
        }
    }

    void UpdateCleaningUI()
    {
        tabletDisplay.UpdateCleaningProgress(cleanedCount, cleaningObjects.Count);
    }

    void OnCleaningComplete()
    {
        Debug.Log("[GameFlow] Cleaning Challenge complete!");
        currentPhase = GamePhase.Complete;

        // Show completion
        tabletDisplay.ShowCleaningComplete();

        // Return to MainMenu after delay
        Invoke(nameof(ReturnToMainMenu), 5f);
    }

    void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    #endregion

    #region ATMOSPHERE

    void StartChallengeAtmosphere()
    {
        // Dim lights (you can connect to your lighting system here)
        Debug.Log("[GameFlow] Dimming lights, starting challenge music");
        
        if (challengeMusicClip != null && ambientAudioSource != null)
        {
            ambientAudioSource.clip = challengeMusicClip;
            ambientAudioSource.Play();
        }
    }

    void EndChallengeAtmosphere()
    {
        // Restore lights and ambient audio
        Debug.Log("[GameFlow] Restoring atmosphere");
        
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }
    }

    #endregion

    #region UTILITIES

    List<string> GetDiscoveredItemIDs()
    {
        // Get all discovered IDs from SentenceHistoryManager
        // Since discoveredIDs is private, we use WordItems in scene
        WordItem[] items = FindObjectsOfType<WordItem>();
        return items.Where(w => SentenceHistoryManager.IsDiscovered(w.objectID))
            .Select(w => w.objectID).Distinct().ToList();
    }

    List<string> GetRandomItems(List<string> source, int count)
    {
        return source.OrderBy(x => Random.value).Take(count).ToList();
    }

    void EnableGrabbing(WordItem item, bool enabled)
    {
        var interactable = item.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.enabled = enabled;
        }
    }

    public GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }

    public bool IsCleaningMode()
    {
        return currentPhase == GamePhase.CleaningChallengeActive;
    }

    #endregion
}
