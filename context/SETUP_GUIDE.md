# Kitchen Scene Integration - Setup Guide

## 🎯 What Was Implemented

### New Scripts Created:
1. **GameFlowController.cs** - Orchestrates 50% Find Challenge & 90% Cleaning Challenge
2. **ArticleCleaningController.cs** - Viscera-style article sorting with actual objects

### Scripts Modified:
1. **QuizGameManager.cs** - Added external challenge trigger support
2. **TabletDisplay.cs** - Added cleaning UI methods
3. **QuizBasket.cs** - Routes to cleaning mode when active

---

## 🔧 Unity Setup Steps

### Step 1: Create GameFlowController GameObject

1. In **Kitchen.unity**, create empty GameObject: `GameFlowController`
2. Add component: **GameFlowController** script
3. Assign references in Inspector:

| Field | Assign To |
|-------|-----------|
| Tablet Display | Tablet Canvas GameObject |
| Quiz Game Manager | Find QuizGameManager in scene (or create if missing) |
| Right Hand Scanner | RightHand Controller GameObject |
| Toilets Parent | Parent of Der/Die/Das toilets (or leave empty to auto-find) |
| Cleaning Spawn Area | Empty GameObject positioned where objects should gather |
| Ambient Audio Source | AudioSource for music (or leave empty) |
| Challenge Music Clip | Your challenge music AudioClip |
| Flush Sound Clip | Your toilet flush AudioClip |

4. Set thresholds:
   - Find Challenge Threshold: `0.5` (50%)
   - Cleaning Threshold: `0.9` (90%)

---

### Step 2: Setup TabletDisplay UI

Add new UI panels to your **Tablet Canvas**:

#### Panel 1: Find Challenge Prompt
1. Create UI Panel: `FindChallengePromptPanel`
2. Add UI Text: "Challenge Available! Find 5 objects quickly!"
3. Add Buttons:
   - "Accept" → calls `TabletDisplay.OnAcceptFindChallenge()`
   - "Later" → calls `TabletDisplay.OnDeclineFindChallenge()`
4. **Disable** this panel by default

#### Panel 2: Cleaning Challenge Prompt  
1. Create UI Panel: `CleaningChallengePromptPanel`
2. Add UI Text: "Kitchen Cleanup Time! Sort objects by article!"
3. Add Buttons:
   - "Start Cleaning" → calls `TabletDisplay.OnAcceptCleaningChallenge()`
   - "Later" → calls `TabletDisplay.OnDeclineCleaningChallenge()`
4. **Disable** this panel by default

#### Panel 3: Cleaning Mode
1. Create UI Panel: `CleaningModePanel`
2. Add UI Text (assign to `cleaningProgressText`): "Cleaned: 0/10"
3. **Disable** this panel by default

#### Assign to TabletDisplay
Select TabletDisplay GameObject, assign:
- Find Challenge Prompt Panel → `findChallengePromptPanel`
- Cleaning Challenge Prompt Panel → `cleaningChallengePromptPanel`
- Cleaning Mode Panel → `cleaningModePanel`
- Progress Text → `cleaningProgressText`

---

### Step 3: Add ArticleCleaningController

1. Create empty GameObject: `ArticleCleaningController`
2. Add component: **ArticleCleaningController** script
3. Assign Toilets (if not auto-finding):
   - Drag `Der Toilet` → `derToilet`
   - Drag `Die Toilet` → `dieToilet`
   - Drag `Das Toilet` → `dasToilet`

---

### Step 4: Ensure QuizGameManager Exists

Make sure **QuizGameManager** is in the scene:
1. Check if there's a GameObject with `QuizGameManager` component
2. If not, create empty GameObject: `QuizGameManager`
3. Add component: **QuizGameManager**
4. Assign UI references (panels, texts) if you want standalone quiz too

---

### Step 5: Setup Toilets for Cleaning

Each toilet needs:
1. **QuizBasket** component
2. `acceptedArticle` field set to: "Der", "Die", or "Das"
3. BoxCollider with **IsTrigger = true**
4. Positioned where objects will fall into

The toilets should already have this from Kitchen_Articel_Quiz scene.

---

### Step 6: Add Toilets to Kitchen Scene

1. Open `Kitchen_Articel_Quiz.unity`
2. Find the 3 toilets (Der/Die/Das) - they're probably children of a parent
3. **Copy** them (Ctrl+C)
4. Open `Kitchen.unity`
5. **Paste** (Ctrl+V)
6. Position them in a corner or designated quiz area
7. Parent them under `ArticleQuiz_System` empty GameObject

---

### Step 7: Test the Flow

1. Enter Play Mode
2. Scan objects until you reach 50%
3. You should see Find Challenge prompt
4. Accept → Play timed hunt
5. Continue scanning until 90%
6. You should see Cleaning Challenge prompt
7. Accept → Objects gather, throw them into correct toilets
8. Completion → Returns to MainMenu

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| No prompt at 50% | Check GameFlowController has TabletDisplay assigned |
| Find Challenge doesn't start | Check QuizGameManager reference is assigned |
| Objects don't flush | Check ArticleCleaningController has toilets assigned |
| Cleaning UI not showing | Check TabletDisplay has cleaning panels assigned |
| Can't grab objects | Ensure WordItems have XRGrabInteractable component |

---

## 🎨 Optional: Polish

1. **Add particle effects** for flush to ArticleCleaningController
2. **Adjust lighting** - Dim lights during challenges (code placeholder is there)
3. **Add sounds** - Assign AudioClips to GameFlowController
4. **Customize UI** - Style the challenge prompts to match your game

---

## 📁 File Locations

New files:
- `Assets/_Project/Scripts/Core/GameFlowController.cs`
- `Assets/_Project/Scripts/Core/ArticleCleaningController.cs`

Modified files:
- `Assets/_Project/Scripts/FindObjectQuiz/QuizGameManager.cs`
- `Assets/_Project/Scripts/TabletDisplay.cs`
- `Assets/_Project/Scripts/ArticelQuiz/QuizBasket.cs`
