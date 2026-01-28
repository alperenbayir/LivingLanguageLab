# Code Validation - One Scene Flow

## ✅ Status: READY TO TEST

All code is clean and handles the one scene flow without scene loading.

---

## 🔍 Code Quality Check

### 1. GameFlowController.cs ✅
- [x] No scene loading during 50% and 90% triggers
- [x] Only loads MainMenu at very end (after 90% complete)
- [x] Properly handles phase transitions
- [x] Auto-finds references if not assigned
- [x] Null checks throughout

### 2. ArticleCleaningController.cs ✅
- [x] No scene loading
- [x] Auto-finds toilets if not assigned
- [x] Handles object flushing with coroutines
- [x] Validates objects are part of cleaning task

### 3. QuizGameManager.cs (Modified) ✅
- [x] Added `StartExternalChallenge()` for 50% trigger
- [x] Returns to GameFlowController on completion
- [x] Backwards compatible with standalone scene mode
- [x] `CancelChallenge()` method for early exit

### 4. TabletDisplay.cs (Modified) ✅
- [x] Added challenge prompt methods
- [x] Added cleaning mode UI
- [x] Old `GoToQuizScene()` still exists but won't be called by new flow
- [x] Proper state management

### 5. QuizBasket.cs (Modified) ✅
- [x] Routes to ArticleCleaningController when in cleaning mode
- [x] Falls back to old QuizManager otherwise
- [x] Backwards compatible

---

## 🎮 Flow Validation

### Test Scenario: Player Journey

```
START: Kitchen.unity
│
├─> Scan objects (0-50%)
│   └─> Tablet shows word, sentence, pronunciation
│
├─> 50% REACHED
│   ├─> GameFlowController.OnObjectDiscovered() called
│   ├─> OfferFindChallenge() shows prompt on tablet
│   ├─> Player ACCEPTS
│   │   └─> StartFindChallenge() → quizGameManager.StartExternalChallenge()
│   │       ├─> Timer starts
│   │       ├─> Find 5 objects
│   │       └─> OnFindChallengeComplete() → back to exploration
│   └─> Player DECLINES
│       └─> Re-offer on next discovery
│
├─> Continue scanning (50-90%)
│   └─> Can retry Find Challenge anytime
│
├─> 90% REACHED
│   ├─> OfferCleaningChallenge() shows prompt
│   ├─> Player ACCEPTS
│   │   └─> StartCleaningChallenge()
│   │       ├─> Gather 10 kitchen objects to spawn area
│   │       ├─> EnterCleaningMode() on tablet
│   │       ├─> Player GRABS objects → THROWS into toilets
│   │       ├─> Correct: flush sound + object destroyed
│   │       └─> All cleaned → OnCleaningComplete()
│   │           └─> ReturnToMainMenu() (5s delay)
│   └─> Player DECLINES
│       └─> Can continue to 100%
│
└─> END
```

**No scene loading during gameplay!** ✅

---

## ⚠️ Critical Setup Required

Before testing, you MUST:

### In Unity Editor - Kitchen.unity:

1. **Create GameFlowController GameObject**
   - Add `GameFlowController` component
   - Assign references (or leave empty for auto-find)

2. **Create ArticleCleaningController GameObject**
   - Add `ArticleCleaningController` component
   - Assign toilets (or leave empty for auto-find)

3. **Copy Toilets from Kitchen_Articel_Quiz.unity**
   - Select Der/Die/Das toilets
   - Copy to Kitchen.unity
   - Position them in quiz area

4. **Add UI Panels to Tablet Canvas**
   - `FindChallengePromptPanel` (disabled by default)
   - `CleaningChallengePromptPanel` (disabled by default)
   - `CleaningModePanel` (disabled by default)
   - Assign to TabletDisplay component

5. **Ensure QuizGameManager exists**
   - May already exist from FindObject scene
   - If not, create empty GameObject and add component

6. **Add LeaderboardManager** (if using leaderboard)
   - Required by QuizGameManager

---

## 🔧 Build Settings (NO CHANGES NEEDED)

You can keep all scenes in Build Settings:
- Kitchen.unity (active flow)
- Kitchen_FindObject_Quiz.unity (standalone backup)
- Kitchen_Articel_Quiz.unity (standalone backup)
- MainMenu.unity

The new code **only uses Kitchen.unity** for the integrated flow.
Standalone scenes remain untouched for direct MainMenu access.

---

## 🐛 Known Limitations (To Fix During Testing)

1. **Lighting/Music**: Placeholder methods - connect your actual systems
2. **Toilet Auto-Find**: Only works if toilets have QuizBasket component
3. **Object Grabbing**: Objects need XRGrabInteractable component
4. **UI Styling**: Panels exist but need visual polish

---

## ✅ Test Checklist

| Feature | Expected | Status |
|---------|----------|--------|
| Scan to 50% | Find Challenge prompt appears | ⬜ Test |
| Accept 50% | Timer starts, find 5 objects | ⬜ Test |
| Complete 50% | Return to exploration | ⬜ Test |
| Decline 50% | Re-offer on next scan | ⬜ Test |
| Scan to 90% | Cleaning prompt appears | ⬜ Test |
| Accept 90% | Objects gather, cleaning UI shows | ⬜ Test |
| Throw to toilet | Correct = flush + destroy | ⬜ Test |
| Complete 90% | Return to MainMenu | ⬜ Test |

---

## 📁 Files Created/Modified

### New Files:
- `Assets/_Project/Scripts/Core/GameFlowController.cs`
- `Assets/_Project/Scripts/Core/ArticleCleaningController.cs`

### Modified Files:
- `Assets/_Project/Scripts/FindObjectQuiz/QuizGameManager.cs`
- `Assets/_Project/Scripts/TabletDisplay.cs`
- `Assets/_Project/Scripts/ArticelQuiz/QuizBasket.cs`

**Total: 5 files changed**

---

## 🚀 Ready to Test!

Follow the setup steps above, then hit Play in Unity.
The flow should work without any scene loading!
