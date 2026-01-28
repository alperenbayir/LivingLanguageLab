# Debug Guide - 30% Trigger Not Working

## 🔍 Step-by-Step Diagnosis

### Step 1: Check GameFlowController Setup

```
1. Select "_Managers" → "GameFlowController"
2. Check Inspector:

   [REQUIRED FIELDS - Must be filled]
   
   ✓ Tablet Display: Must show "Tablet Canvas (TabletDisplay)"
   ✓ Quiz Game Manager: Must show "Tablet Origin (QuizGameManager)"
   ✓ Find Challenge Threshold: Must show "0.3"
   
   [OPTIONAL FIELDS - Can be empty for now]
   
   ○ Right Hand Scanner: Can be empty
   ○ Toilets Parent: Can be empty  
   ○ Cleaning Spawn Area: Can be empty
```

**If Tablet Display shows "None":**
- Drag "Tablet Canvas" from Hierarchy to this field

**If Quiz Game Manager shows "None":**
- Drag "Tablet Origin" (where QuizGameManager is) to this field

---

### Step 2: Verify UI Panels Exist

```
Expand in Hierarchy: Tablet Canvas → Canvas Layout

Do you see these panels?
├─ FindQuizLayout (copied from FindObject scene)
├─ FindChallengePromptPanel [NEW - you created this]
├─ CleaningChallengePromptPanel [NEW - you created this]
└─ CleaningModePanel [NEW - you created this]

Check:
- Are they created?
- Are they DISABLED (box unchecked)? ← Should be disabled by default
```

**If missing:** Create them (see MIGRATION_GUIDE.md Step 9-11)

---

### Step 3: Check TabletDisplay References

```
1. Select "Tablet Canvas"
2. Find "Tablet Display" component
3. Scroll to bottom - look for new fields:

   Game Flow Panels (should show)
   ├─ Find Challenge Prompt Panel: [should show your panel]
   ├─ Cleaning Challenge Prompt Panel: [should show your panel]
   ├─ Cleaning Mode Panel: [should show your panel]
   └─ ...

If these show "None":
- Drag the panels you created to these fields
```

---

### Step 4: Enable Debug Logs

Open Console window (Ctrl+Shift+C) and check for:

**GOOD Messages (should appear):**
```
[GameFlow] Offering Find Challenge at 50%
[GameFlow] Starting Find Challenge
[GameFlow] Find Challenge declined
```

**BAD Messages (errors):**
```
NullReferenceException: Object reference not set
→ Some reference is missing

MissingReferenceException: The object of type 'GameObject' has been destroyed
→ Reference pointing to deleted object
```

---

### Step 5: Quick Test with Debug

Let's add temporary debug output:

**Option A: Check in Play Mode**
```
1. Enter Play Mode
2. Scan an object
3. Look at GameFlowController in Inspector
4. Check if values change:
   - Current Phase should show "Exploration"
   - Find Challenge Offered should be "false"
```

**Option B: Force the trigger**
```
In GameFlowController Inspector:
1. Change "Find Challenge Threshold" to 0.1 (10%)
2. Enter Play Mode
3. Scan ONE object
4. Should trigger at 10% (1/10 items)
```

---

### Step 6: Check Button Callbacks

Your prompt panel buttons must call the right functions:

**Accept Button:**
```
On Click ()
├─ Runtime Only
├─ Tablet Canvas (drag here)
└─ TabletDisplay.OnAcceptFindChallenge () ← MUST BE THIS
```

**Decline Button:**
```
On Click ()
├─ Runtime Only
├─ Tablet Canvas (drag here)
└─ TabletDisplay.OnDeclineFindChallenge () ← MUST BE THIS
```

**Common Mistake:**
- Button still calls old `TabletDisplay.GoToQuizScene()`
- Must change to `OnAcceptFindChallenge` or `OnDeclineFindChallenge`

---

### Step 7: Verify the Flow Chain

The chain should be:

```
1. Player scans object
   ↓
2. RightHandScanner.ScanCurrentHover()
   ↓
3. TabletDisplay.UpdateDisplay(item)
   ↓
4. CheckProgressForQuiz()
   ↓
5. GameFlowController.OnObjectDiscovered(id)
   ↓
6. GameFlowController.OfferFindChallenge()
   ↓
7. TabletDisplay.ShowFindChallengePrompt()
   ↓
8. Panel becomes visible!
```

**Where is it breaking?**

Check Console after scanning:
- No messages? → Step 3-4 broken
- Only "[Learn] Kelime gosteriliyor"? → Step 4-5 broken
- "[GameFlow] Offering..." but no UI? → Step 7-8 broken

---

## 🐛 Common Issues & Fixes

### Issue 1: "GameFlowController.Instance is null"
**Fix:** You didn't create GameFlowController GameObject with component

### Issue 2: Panel appears but empty/black
**Fix:** Panel exists but has no background image/color
- Select panel → Add Image component or change color

### Issue 3: Buttons don't work
**Fix:** Wrong function assigned
- Check OnClick() callback
- Should be `TabletDisplay.OnAcceptFindChallenge`
- NOT `GoToQuizScene`

### Issue 4: Nothing happens at all
**Fix:** GameFlowController not receiving discovery event
- Check Tablet Display has GameFlowController assigned
- Or check code change was applied (CheckProgressForQuiz)

### Issue 5: Old prompt still appears
**Fix:** You have QuizOfferPanel still active
- Disable or delete QuizOfferPanel
- Only FindChallengePromptPanel should be used

---

## 🧪 Emergency Debug Script

If still not working, add this temporarily to GameFlowController:

```csharp
void Update()
{
    // Press T to test trigger manually
    if (Input.GetKeyDown(KeyCode.T))
    {
        Debug.Log("[TEST] Manual trigger!");
        OfferFindChallenge();
    }
}
```

Then in Play Mode:
- Press T key → Should show prompt immediately
- If works → Problem is progress detection
- If not works → Problem is UI/panel setup

---

## 📋 Quick Checklist

- [ ] GameFlowController GameObject exists
- [ ] GameFlowController component added
- [ ] Tablet Display field assigned
- [ ] Quiz Game Manager field assigned
- [ ] Find Challenge Threshold = 0.3 (or 0.1 for test)
- [ ] FindChallengePromptPanel created
- [ ] FindChallengePromptPanel assigned to TabletDisplay
- [ ] Buttons call OnAcceptFindChallenge / OnDeclineFindChallenge
- [ ] QuizOfferPanel (old) is DISABLED
- [ ] No errors in Console

**Which items are NOT checked? That's your problem!**
