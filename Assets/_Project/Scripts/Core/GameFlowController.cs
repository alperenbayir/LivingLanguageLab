using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Orchestrates the 3-phase Kitchen experience:
/// 1. Exploration (0-80%) - Free scanning
/// 2. Find Challenge (~15% intervals) - Timer-based hunt, non-destructive
/// 3. Article Sorting Finale (80%) - Sort objects into Der/Die/Das baskets
/// </summary>
public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance;

    [Header("Phase Settings - Find Game")]
    private float[] findChallengeMilestones = { 0.067f, 0.20f }; // ~5 scans, ~15 scans (out of 75)
    private int requiredFindChallenges = 1;

    [Header("Phase Settings - Article Sorting")]
    private float[] cleaningMilestones = { 0.27f }; // ~20 scans (out of 75)

    [Header("Highlight Settings")]
    public Color highlightColor = new Color(0.3f, 1f, 0.3f, 1f); // Subtle green glow
    [Range(0.05f, 0.3f)]
    public float highlightIntensity = 0.1f; // Keep it subtle

    [Header("FindObject Challenge Settings")]
    private int findChallengeItemCount = 8;
    public bool findChallengeTimed = true;

    [Header("Article Sorting Challenge Settings")]
    private int cleaningObjectCount = 6;
    public Transform cleaningSpawnArea; // Area to gather objects initially

    [Header("Audio")]
    public AudioSource ambientAudioSource;
    public AudioClip ambientMusicClip;
    public AudioClip challengeMusicClip;
    
    [Header("Lighting")]
    public Light mainLight;
    public float normalLightIntensity = 1f;
    public float challengeLightIntensity = 0.3f;
    public Color normalLightColor = Color.white;
    public Color challengeLightColor = new Color(0.8f, 0.6f, 1f); // Slight purple tint
    
    [Header("Disco Lights")]
    public Light[] challengeDiscoLights; // Assign multiple Spot Lights here for disco effect

    [Header("References")]
    public TabletDisplay tabletDisplay;
    public QuizGameManager quizGameManager;
    public RightHandScanner rightHandScanner;

    // Internal state
    private int findChallengesCompleted = 0;
    private int currentFindMilestoneIndex = 0; // 0=15%, 1=30%, 2=45%
    private int currentCleaningMilestoneIndex = 0; // 0=70%, 1=100%
    private int cleaningChallengesCompleted = 0;
    private GamePhase currentPhase = GamePhase.Exploration;
    private List<WordItem> discoveredItems = new List<WordItem>();
    private List<WordItem> cleaningObjects = new List<WordItem>();
    private List<string> cleaningItemNames = new List<string>();
    private HashSet<string> sortedItemNames = new HashSet<string>(); // For strikethrough display
    private HashSet<string> globallySortedItemIDs = new HashSet<string>(); // Persists across rounds
    private HashSet<string> usedFindChallengeItemIDs = new HashSet<string>(); // No repeats across Find challenges
    private int cleanedCount = 0;

    // Progress tracking
    private bool isWaitingForResponse = false;

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

    [Header("Debug")]
    [HideInInspector] public bool debugSkipToSorting = false;

    void Start()
    {
        // Clear old leaderboard data for fresh session (VR demo - new players each time)
        PlayerPrefs.DeleteKey("KitchenQuizLeaderboard");
        PlayerPrefs.Save();
        Debug.Log("[GameFlow] Leaderboard cleared - fresh session started!");
        
        // Validate references
        if (tabletDisplay == null)
            tabletDisplay = FindObjectOfType<TabletDisplay>();
        if (quizGameManager == null)
            quizGameManager = FindObjectOfType<QuizGameManager>();
        if (rightHandScanner == null)
            rightHandScanner = FindObjectOfType<RightHandScanner>();

        currentPhase = GamePhase.Exploration;
        
        // Ensure all disco lights are OFF at game start
        foreach (Light light in challengeDiscoLights)
        {
            if (light != null) light.gameObject.SetActive(false);
        }
        
        // Start ambient music
        if (ambientAudioSource != null && ambientMusicClip != null)
        {
            ambientAudioSource.clip = ambientMusicClip;
            ambientAudioSource.volume = 0.5f;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }

        // Debug: Skip directly to sorting challenge for testing
        if (debugSkipToSorting)
        {
            Invoke(nameof(DebugStartSorting), 2f); // Delay to let scene initialize
        }
    }

    void DebugStartSorting()
    {
        Debug.Log("[GameFlow] DEBUG: Starting sorting challenge prompt");
        findChallengesCompleted = requiredFindChallenges; // Skip find challenge requirement
        OfferCleaningChallenge(); // Show prompt first
    }

    /// <summary>
    /// Called by TabletDisplay when new object is discovered
    /// </summary>
    public void OnObjectDiscovered(string objectID)
    {
        // Don't interrupt if challenge is active or waiting for response
        if (currentPhase == GamePhase.FindChallengeActive ||
            currentPhase == GamePhase.CleaningChallengeActive ||
            isWaitingForResponse)
        {
            return;
        }

        float progress = GetDiscoveryProgress();
        bool hasCompletedEnoughFindChallenges = findChallengesCompleted >= requiredFindChallenges;

        // Check for Find Challenge triggers (15%, 30%, 45%)
        if (currentFindMilestoneIndex < findChallengeMilestones.Length)
        {
            float nextMilestone = findChallengeMilestones[currentFindMilestoneIndex];

            if (progress >= nextMilestone)
            {
                bool isLastMilestone = (currentFindMilestoneIndex == findChallengeMilestones.Length - 1);
                bool mustForce = isLastMilestone && findChallengesCompleted < requiredFindChallenges;

                Debug.Log($"[GameFlow] Find Challenge at {nextMilestone:P0} (force: {mustForce}, completed: {findChallengesCompleted}/{requiredFindChallenges})");
                currentFindMilestoneIndex++;
                OfferFindChallenge(forcePlay: mustForce);
            }
        }
        // Check for Article Sorting Challenge (70%, 100%) - after completing required Find Challenges
        else if (hasCompletedEnoughFindChallenges && currentCleaningMilestoneIndex < cleaningMilestones.Length)
        {
            float nextMilestone = cleaningMilestones[currentCleaningMilestoneIndex];

            if (progress >= nextMilestone)
            {
                bool isLastMilestone = (currentCleaningMilestoneIndex == cleaningMilestones.Length - 1);
                bool mustForce = isLastMilestone && cleaningChallengesCompleted < 1; // Force at 100% if not done

                Debug.Log($"[GameFlow] Article Sorting at {nextMilestone:P0} (force: {mustForce}, completed: {cleaningChallengesCompleted})");
                currentCleaningMilestoneIndex++;
                OfferCleaningChallenge(forcePlay: mustForce);
            }
        }
    }

    float GetDiscoveryProgress()
    {
        if (VocabularyManager.Instance == null) return 0f;
        float discovered = SentenceHistoryManager.GetDiscoveredCount();
        float total = VocabularyManager.Instance.GetTotalCount();
        return discovered / total;
    }

    #region FIND OBJECT CHALLENGE

    void OfferFindChallenge(bool forcePlay = false)
    {
        Debug.Log($"[GameFlow] Offering Find Challenge (force: {forcePlay}, completed: {findChallengesCompleted}/{requiredFindChallenges})");

        isWaitingForResponse = true;
        RightHandScanner.CanScan = false;
        StartChallengeAtmosphere();

        // Show offer on tablet (hide decline button if forced)
        tabletDisplay.ShowFindChallengePrompt(
            onAccept: () => {
                isWaitingForResponse = false;
                StartFindChallenge();
            },
            onDecline: () => {
                Debug.Log("[GameFlow] Find Challenge declined");
                isWaitingForResponse = false;
                EndChallengeAtmosphere();
                RightHandScanner.CanScan = true;
                if (tabletDisplay != null)
                    tabletDisplay.ReturnToExplorationMode();
            },
            forcePlay: forcePlay
        );
    }

    void StartFindChallenge()
    {
        Debug.Log("[GameFlow] Starting Find Challenge");
        currentPhase = GamePhase.FindChallengeActive;
        
        // Enable all disco lights when challenge actually starts (Yes pressed)
        foreach (Light light in challengeDiscoLights)
        {
            if (light != null) light.gameObject.SetActive(true);
        }
        
        // Re-enable scanning so player can find objects
        RightHandScanner.CanScan = true;

        // Get discovered items, excluding already used ones (no repeats across challenges)
        List<string> discoveredIDs = GetDiscoveredItemIDs();
        List<string> availableIDs = discoveredIDs
            .Where(id => !usedFindChallengeItemIDs.Contains(id))
            .ToList();

        List<string> challengeItems = GetRandomItems(availableIDs, findChallengeItemCount);

        // Track used items so they won't appear in future challenges
        foreach (string id in challengeItems)
        {
            usedFindChallengeItemIDs.Add(id);
        }

        // Start the quiz with external trigger
        quizGameManager.StartExternalChallenge(challengeItems, findChallengeTimed);
    }

    public void OnFindChallengeComplete(float finalTime)
    {
        findChallengesCompleted++;
        Debug.Log($"[GameFlow] Find Challenge complete! Time: {finalTime:F1}s (Total completed: {findChallengesCompleted}/{requiredFindChallenges})");

        isWaitingForResponse = false;
        currentPhase = GamePhase.Exploration;

        EndChallengeAtmosphere();
        RightHandScanner.CanScan = true;

        if (tabletDisplay != null)
        {
            tabletDisplay.ReturnToExplorationMode();
            string message = findChallengesCompleted >= requiredFindChallenges
                ? $"Great! Time: {finalTime:F1}s\nArticle Sorting unlocked!"
                : $"Good! Time: {finalTime:F1}s\nKeep exploring...";
            tabletDisplay.ShowMessage(message);
        }
    }

    public void OnFindChallengeCancelled()
    {
        Debug.Log("[GameFlow] Find Challenge cancelled");
        isWaitingForResponse = false;
        currentPhase = GamePhase.Exploration;
        EndChallengeAtmosphere();
        
        // Return to exploration mode
        if (tabletDisplay != null)
        {
            tabletDisplay.ReturnToExplorationMode();
        }
    }

    #endregion

    #region ARTICLE SORTING CHALLENGE (50%)

    void OfferCleaningChallenge(bool forcePlay = false)
    {
        Debug.Log($"[GameFlow] Offering Article Sorting Challenge (forcePlay: {forcePlay})");

        // Lock state to prevent overlapping UI
        isWaitingForResponse = true;
        RightHandScanner.CanScan = false;

        // Dramatic atmosphere
        StartChallengeAtmosphere();

        // Show prompt (hide decline button if forced)
        tabletDisplay.ShowCleaningChallengePrompt(
            onAccept: () => {
                isWaitingForResponse = false;
                // Scanning stays disabled during sorting challenge
                StartCleaningChallenge();
            },
            onDecline: () => {
                Debug.Log("[GameFlow] Article Sorting Challenge declined - will ask again later");
                isWaitingForResponse = false;
                RightHandScanner.CanScan = true; // Re-enable scanning
                EndChallengeAtmosphere();
                if (tabletDisplay != null)
                    tabletDisplay.ReturnToExplorationMode();
            },
            forcePlay: forcePlay
        );
    }

    void StartCleaningChallenge()
    {
        Debug.Log("[GameFlow] Starting Article Sorting Challenge");
        currentPhase = GamePhase.CleaningChallengeActive;
        cleanedCount = 0;

        // Enable basket area (parent must be active for children to show)
        if (cleaningSpawnArea != null)
        {
            cleaningSpawnArea.gameObject.SetActive(true);
            Debug.Log("[GameFlow] Enabled BasketSpawnArea");
        }

        // Collect actual kitchen objects for cleaning
        cleaningObjects = GetKitchenObjectsForCleaning(cleaningObjectCount);

        // Gather them to cleaning area and highlight them
        GatherObjectsForCleaning();
        HighlightCleaningObjects(true);

        // Setup tablet for cleaning UI with item names (without articles, comma-separated)
        cleaningItemNames = cleaningObjects
            .Select(w => {
                var item = VocabularyManager.Instance?.GetItem(w.objectID);
                if (item != null && item.german.Contains(" "))
                    return item.german.Substring(item.german.IndexOf(' ') + 1); // Remove article
                return item?.german ?? w.objectID;
            })
            .ToList();
        sortedItemNames.Clear();
        tabletDisplay.EnterCleaningMode(cleaningObjects.Count, cleaningItemNames);

        // Enable cleaning interaction (this enables individual baskets)
        ArticleCleaningController.Instance?.StartCleaningMode(cleaningObjects);

        UpdateCleaningUI();
    }

    List<WordItem> GetKitchenObjectsForCleaning(int count)
    {

        // Get all WordItems in scene
        WordItem[] allItems = FindObjectsByType<WordItem>(FindObjectsSortMode.None);

        // Filter: canSort=true, not already sorted globally, unique by objectID
        var sortable = allItems
            .Where(w => VocabularyManager.Instance.CanSort(w.objectID))
            .Where(w => !globallySortedItemIDs.Contains(w.objectID)) // Exclude items sorted in previous rounds
            .GroupBy(w => w.objectID)
            .Select(g => g.First())
            .ToList();

        // Separate discovered and undiscovered
        var discovered = sortable.Where(w => SentenceHistoryManager.IsDiscovered(w.objectID)).ToList();
        var undiscovered = sortable.Where(w => !SentenceHistoryManager.IsDiscovered(w.objectID)).ToList();

        // Prioritize discovered items, then fill with undiscovered
        var selected = discovered.OrderBy(x => Random.value)
            .Concat(undiscovered.OrderBy(x => Random.value))
            .Take(count)
            .ToList();

        Debug.Log($"[GameFlow] Selected {selected.Count} unique sortable objects for cleaning challenge");

        return selected;
    }

    void GatherObjectsForCleaning()
    {
        // Enable grabbing on all cleaning objects (they stay in their original scene positions)
        foreach (var item in cleaningObjects)
        {
            if (item != null)
            {
                EnableGrabbing(item, true);
            }
        }
    }

    public void OnObjectCleaned(WordItem item)
    {
        cleanedCount++;

        // Mark item as sorted for strikethrough on tablet + track globally
        if (item != null)
        {
            globallySortedItemIDs.Add(item.objectID); // Persists across rounds

            var vocabItem = VocabularyManager.Instance?.GetItem(item.objectID);
            if (vocabItem != null)
            {
                string itemName = vocabItem.german.Contains(" ")
                    ? vocabItem.german.Substring(vocabItem.german.IndexOf(' ') + 1)
                    : vocabItem.german;
                sortedItemNames.Add(itemName);
            }
        }

        UpdateCleaningUI();

        // Check completion
        if (cleanedCount >= cleaningObjects.Count)
        {
            OnCleaningComplete();
        }
    }

    void UpdateCleaningUI()
    {
        tabletDisplay.UpdateCleaningProgress(cleanedCount, cleaningObjects.Count);
        tabletDisplay.UpdateCleaningItemsList(cleaningItemNames, sortedItemNames);
    }

    void OnCleaningComplete()
    {
        cleaningChallengesCompleted++;
        Debug.Log($"[GameFlow] Article Sorting complete! (Total: {cleaningChallengesCompleted}, Globally sorted: {globallySortedItemIDs.Count})");

        EndChallengeAtmosphere();
        RightHandScanner.CanScan = true;

        currentPhase = GamePhase.Complete;
        tabletDisplay.ShowCleaningComplete();
        StartCoroutine(FoodComboCountdown());
    }

    IEnumerator FoodComboCountdown()
    {
        if (tabletDisplay.transitionUI != null)
            tabletDisplay.transitionUI.SetActive(true);

        for (int i = 5; i >= 1; i--)
        {
            if (tabletDisplay.timeCountdownText != null)
                tabletDisplay.timeCountdownText.text = $"Starting in {i}..";
            yield return new WaitForSeconds(1f);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Kitchen_FoodComboGame");
    }

    #endregion

    #region ATMOSPHERE

    void StartChallengeAtmosphere()
    {
        Debug.Log("[GameFlow] Dimming lights, starting challenge music");
        
        // Dim the main light
        if (mainLight != null)
        {
            mainLight.intensity = challengeLightIntensity;
            mainLight.color = challengeLightColor;
        }
        
        // Note: Disco light is enabled in StartFindChallenge() when player presses Yes
        
        // Switch to challenge music
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
            if (challengeMusicClip != null)
            {
                ambientAudioSource.clip = challengeMusicClip;
                ambientAudioSource.volume = 0.7f;
                ambientAudioSource.Play();
            }
        }
    }

    void EndChallengeAtmosphere()
    {
        Debug.Log("[GameFlow] Restoring atmosphere");
        
        // Restore normal lighting
        if (mainLight != null)
        {
            mainLight.intensity = normalLightIntensity;
            mainLight.color = normalLightColor;
        }
        
        // Disable all disco lights
        foreach (Light light in challengeDiscoLights)
        {
            if (light != null) light.gameObject.SetActive(false);
        }
        
        // Return to ambient music
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
            if (ambientMusicClip != null)
            {
                ambientAudioSource.clip = ambientMusicClip;
                ambientAudioSource.volume = 0.5f;
                ambientAudioSource.loop = true;
                ambientAudioSource.Play();
            }
        }
    }

    #endregion

    #region UTILITIES

    List<string> GetDiscoveredItemIDs()
    {
        // Get all discovered IDs from SentenceHistoryManager
        // Since discoveredIDs is private, we use WordItems in scene
        WordItem[] items = FindObjectsByType<WordItem>(FindObjectsSortMode.None);
        return items.Where(w => SentenceHistoryManager.IsDiscovered(w.objectID))
            .Select(w => w.objectID).Distinct().ToList();
    }

    List<string> GetRandomItems(List<string> source, int count)
    {
        return source.OrderBy(x => Random.value).Take(count).ToList();
    }

    void EnableGrabbing(WordItem item, bool enabled)
    {
        // FIRST: Disable XR Simple Interactable (must happen before enabling grab)
        var simpleInteractable = item.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (simpleInteractable != null)
        {
            simpleInteractable.enabled = !enabled;
            Debug.Log($"[GameFlow] {item.objectID} XRSimpleInteractable enabled = {!enabled}");
        }

        // THEN: Enable XR Grab Interactable
        var grabInteractable = item.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = enabled;
            Debug.Log($"[GameFlow] {item.objectID} XRGrabInteractable enabled = {enabled}");
        }
        else
        {
            Debug.LogWarning($"[GameFlow] {item.objectID} has no XRGrabInteractable!");
        }

        // Enable physics for grabbing
        var rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = !enabled;
            Debug.Log($"[GameFlow] {item.objectID} Rigidbody isKinematic = {!enabled}");
        }
        else
        {
            Debug.LogWarning($"[GameFlow] {item.objectID} has no Rigidbody!");
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

    void HighlightCleaningObjects(bool highlight)
    {
        foreach (var item in cleaningObjects)
        {
            if (item == null) continue;
            SetObjectHighlight(item, highlight);
        }
    }

    void SetObjectHighlight(WordItem item, bool highlight)
    {
        if (item == null) return;

        var renderers = item.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                if (highlight)
                {
                    // Subtle emission only - don't change base color to preserve texture
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", highlightColor * highlightIntensity);
                }
                else
                {
                    mat.DisableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }

    public void RemoveHighlight(WordItem item)
    {
        SetObjectHighlight(item, false);
    }

    #endregion
}
