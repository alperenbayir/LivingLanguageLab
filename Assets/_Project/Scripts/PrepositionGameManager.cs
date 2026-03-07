using System.Collections.Generic;
using UnityEngine;


public class PrepositionGameManager : MonoBehaviour
{
    [Header("Tablet")]
    public TabletDisplay tablet; // Drag Tablet Origin here

    [Header("Zones (drag all zone objects here)")]
    public List<PrepositionZone> zones = new();

    [Header("Target Object")]
    public string targetTag = "Plate";

    [Header("Timing")]
    [Tooltip("How long the plate must stay in the correct zone (ONLY while NOT grabbed).")]
    public float holdTime = 0.8f;

    [Tooltip("How long 'Gut gemacht!' stays visible.")]
    public float successMessageSeconds = 5.0f;

    [Tooltip("How long the wrong message stays visible before going back to the instruction.")]
    public float wrongMessageSeconds = 2.0f;

    [Header("Scoring / Level Rules")]
    [Range(0, 100)]
    public int passPercentage = 60;

    [Header("Wrong Feedback")]
    public bool showWrongFeedback = true;

    [Tooltip("Minimum seconds between wrong messages (prevents spam).")]
    public float repeatWrongMessageCooldown = 1.0f;

    [Header("Round Behavior")]
    public bool stopAfterRoundEnds = true;

    [Tooltip("If true, UNDER always appears last.")]
    public bool underAlwaysLast = true;

    [Header("Debug (optional)")]
    public bool showDebugLogs = false;

    public PrepositionType currentTarget;

    private float correctTimer = 0f;
    private bool isShowingSuccess = false;
    private bool roundEnded = false;

    private readonly List<PrepositionType> remainingTargets = new();

    private int totalChallenges = 0;
    private int completedCorrect = 0;

    private float wrongCooldownTimer = 0f;

    private GameObject targetObj;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable targetGrab;

    private bool wasGrabbedLastFrame = false;

    // NEW: make sure we set a German instruction at the very start
    private void Awake()
    {
        if (tablet == null) tablet = FindObjectOfType<TabletDisplay>();

        // Force the tablet into Scanning so sentenceText is visible
        if (tablet != null)
            tablet.SetState(TabletDisplay.TabletMode.Scanning);

        // Clear any old/previous text that might still be on the tablet (often English from other systems)
        ForceGermanText("...");
    }

    private void Start()
    {
        if (tablet == null) tablet = FindObjectOfType<TabletDisplay>();

        foreach (var z in zones)
            if (z != null) z.targetTag = targetTag;

        if (tablet != null)
            tablet.SetState(TabletDisplay.TabletMode.Scanning);

        CacheTarget();

        BuildRoundPool();
        totalChallenges = remainingTargets.Count;
        completedCorrect = 0;
        UpdateScoreUI();

        // NEW: pick first target randomly (NOT always ON)
        PickNextTarget();
        UpdateInstruction();

        // NEW: Force again right after start to overwrite any last-second English text
        // (some scripts write to the tablet in Start as well)
        Invoke(nameof(ForceUpdateInstruction), 0.05f);
    }

    private void ForceUpdateInstruction()
    {
        if (roundEnded) return;
        if (isShowingSuccess) return;
        UpdateInstruction();
    }

    private void CacheTarget()
    {
        targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj == null)
        {
            Debug.LogWarning($"[PrepositionGameManager] No object found with tag '{targetTag}'.");
            return;
        }

        targetGrab = targetObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (targetGrab == null) targetGrab = targetObj.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (targetGrab == null) targetGrab = targetObj.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void Update()
    {
        if (roundEnded) return;
        if (isShowingSuccess) return;

        if (targetObj == null || targetGrab == null)
            CacheTarget();

        wrongCooldownTimer = Mathf.Max(0f, wrongCooldownTimer - Time.deltaTime);

        bool grabbedNow = IsTargetGrabbed();
        bool releasedThisFrame = wasGrabbedLastFrame && !grabbedNow;
        wasGrabbedLastFrame = grabbedNow;

        var correctZone = GetZone(currentTarget);
        if (correctZone == null) return;

        // WRONG FEEDBACK (only when released)
        if (showWrongFeedback && releasedThisFrame)
        {
            var wrongZone = GetAnyZoneContainingTarget(exceptType: currentTarget);

            if (wrongZone != null && !correctZone.containsTarget && wrongCooldownTimer <= 0f)
            {
                wrongCooldownTimer = repeatWrongMessageCooldown;
                ShowWrongMessage(wrongZone.zoneType);
            }
        }

        // COUNT ONLY WHEN IN CORRECT ZONE AND NOT GRABBED
        if (correctZone.containsTarget && !grabbedNow)
        {
            correctTimer += Time.deltaTime;

            if (showDebugLogs)
                Debug.Log($"[GM] timer={correctTimer:0.00}/{holdTime:0.00} target={currentTarget}");

            if (correctTimer >= holdTime)
                OnSuccess();
        }
        else
        {
            correctTimer = 0f;
        }
    }

    private bool IsTargetGrabbed()
    {
        if (targetGrab == null) return false;
        return targetGrab.isSelected;
    }

    private PrepositionZone GetZone(PrepositionType type)
    {
        foreach (var z in zones)
            if (z != null && z.zoneType == type)
                return z;
        return null;
    }

    private PrepositionZone GetAnyZoneContainingTarget(PrepositionType exceptType)
    {
        foreach (var z in zones)
        {
            if (z == null) continue;
            if (z.zoneType == exceptType) continue;
            if (z.containsTarget) return z;
        }
        return null;
    }

    private string ZoneWhereGerman(PrepositionType type)
    {
        return type switch
        {
            PrepositionType.On => "AUF der Mikrowelle",
            PrepositionType.In => "IN der Mikrowelle",
            PrepositionType.Under => "UNTER der Mikrowelle",
            PrepositionType.Behind => "HINTER der Mikrowelle",
            PrepositionType.Left => "LINKS von der Mikrowelle",
            PrepositionType.Right => "RECHTS von der Mikrowelle",
            _ => "an einer falschen Stelle"
        };
    }

    private string TargetInstructionGerman(PrepositionType target)
    {
        return target switch
        {
            PrepositionType.On => "Lege den Teller AUF die Mikrowelle.",
            PrepositionType.In => "Lege den Teller IN die Mikrowelle.",
            PrepositionType.Under => "Lege den Teller UNTER die Mikrowelle.",
            PrepositionType.Behind => "Lege den Teller HINTER die Mikrowelle.",
            PrepositionType.Left => "Lege den Teller LINKS von der Mikrowelle ab.",
            PrepositionType.Right => "Lege den Teller RECHTS von der Mikrowelle ab.",
            _ => "Bewege den Teller."
        };
    }

    private void ShowWrongMessage(PrepositionType wrongType)
    {
        if (isShowingSuccess) return;

        string wrongWhere = ZoneWhereGerman(wrongType);
        string mustDo = TargetInstructionGerman(currentTarget);

        ForceGermanText(
            $"<color=#FF5555>Nicht ganz richtig.</color>\n" +
            $"Der Teller ist <b>{wrongWhere}</b>.\n" +
            $"<color=#00D1FF>Richtig ist:</color> <b>{mustDo}</b>\n" +
            $"(Loslassen und {holdTime:0.0}s warten.)"
        );

        CancelInvoke(nameof(RestoreInstructionFromWrong));
        Invoke(nameof(RestoreInstructionFromWrong), wrongMessageSeconds);
    }

    private void RestoreInstructionFromWrong()
    {
        if (roundEnded) return;
        if (isShowingSuccess) return;
        UpdateInstruction();
    }

    private void OnSuccess()
    {
        if (isShowingSuccess) return;

        isShowingSuccess = true;
        correctTimer = 0f;

        completedCorrect++;
        UpdateScoreUI();

        CancelInvoke(nameof(RestoreInstructionFromWrong));

        ForceGermanText("<color=#00FF88>Gut gemacht!</color>");

        CancelInvoke(nameof(AdvanceAfterSuccess));
        Invoke(nameof(AdvanceAfterSuccess), successMessageSeconds);
    }

    private void AdvanceAfterSuccess()
    {
        isShowingSuccess = false;

        if (!PickNextTarget())
        {
            EndRound();
            return;
        }

        UpdateInstruction();
    }

    private void BuildRoundPool()
    {
        remainingTargets.Clear();
        remainingTargets.Add(PrepositionType.On);
        remainingTargets.Add(PrepositionType.In);
        remainingTargets.Add(PrepositionType.Behind);
        remainingTargets.Add(PrepositionType.Left);
        remainingTargets.Add(PrepositionType.Right);
        remainingTargets.Add(PrepositionType.Under);
    }

    private bool PickNextTarget()
    {
        if (remainingTargets.Count == 0)
            return false;

        // UNDER last: exclude it until it's the only one left
        if (underAlwaysLast && remainingTargets.Count > 1 && remainingTargets.Contains(PrepositionType.Under))
        {
            List<PrepositionType> selectable = new();
            foreach (var p in remainingTargets)
                if (p != PrepositionType.Under)
                    selectable.Add(p);

            currentTarget = selectable[Random.Range(0, selectable.Count)];
            remainingTargets.Remove(currentTarget);
            return true;
        }

        int idx = Random.Range(0, remainingTargets.Count);
        currentTarget = remainingTargets[idx];
        remainingTargets.RemoveAt(idx);
        return true;
    }

    private void EndRound()
    {
        roundEnded = true;

        float pct = (totalChallenges <= 0) ? 0f : (completedCorrect * 100f / totalChallenges);
        bool passed = pct >= passPercentage;

        PlayerPrefs.SetInt("PrepositionPassed", passed ? 1 : 0);
        PlayerPrefs.SetFloat("PrepositionScore", pct);
        PlayerPrefs.Save();

        UpdateScoreUI(finalPercent: pct);

        if (passed)
        {
            ForceGermanText(
                $"<color=#00FF88>Level abgeschlossen!</color>\n" +
                $"Punktzahl: <b>{completedCorrect}/{totalChallenges}</b> ({pct:0}% )\n" +
                $"Du kannst jetzt zum nächsten Level wechseln oder weiter üben."
            );
        }
        else
        {
            ForceGermanText(
                $"<color=#FF5555>Versuche es noch einmal!</color>\n" +
                $"Punktzahl: <b>{completedCorrect}/{totalChallenges}</b> ({pct:0}% )\n" +
                $"Du brauchst mindestens <b>{passPercentage}%</b>, um weiterzukommen."
            );
        }

        if (!stopAfterRoundEnds)
        {
            // optional endless mode: RestartRound();
        }
    }

    public void RestartRound()
    {
        roundEnded = false;
        isShowingSuccess = false;
        correctTimer = 0f;
        completedCorrect = 0;

        CancelInvoke(nameof(RestoreInstructionFromWrong));

        BuildRoundPool();
        totalChallenges = remainingTargets.Count;
        UpdateScoreUI();

        PickNextTarget();
        UpdateInstruction();
    }

    private void UpdateInstruction()
    {
        string msg = TargetInstructionGerman(currentTarget);
        ForceGermanText(msg + $"\n(Loslassen und {holdTime:0.0}s warten.)");
    }

    // This method always writes to the SAME place the tablet shows for you (Sentence Text)
    private void ForceGermanText(string text)
    {
        if (tablet != null && tablet.sentenceText != null)
            tablet.sentenceText.text = text;
    }

    private void UpdateScoreUI(float? finalPercent = null)
    {
        if (tablet != null && tablet.progressText != null)
        {
            if (finalPercent.HasValue)
                tablet.progressText.text = $"{completedCorrect}/{totalChallenges} ({finalPercent.Value:0}%)";
            else
            {
                float pct = (totalChallenges <= 0) ? 0f : (completedCorrect * 100f / totalChallenges);
                tablet.progressText.text = $"{completedCorrect}/{totalChallenges} ({pct:0}%)";
            }
        }
    }
}
