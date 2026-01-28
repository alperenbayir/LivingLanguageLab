# Complete Migration Guide - One Clean Scene

## 🎯 Goal: Kitchen.unity = Everything

## 📋 Phase 1: Preparation (Do NOT skip!)

### Step 1: Backup Your Project
```
Before touching anything:
1. Close Unity
2. Copy entire project folder to backup
3. Or use Git: git add . && git commit -m "Before kitchen migration"
```

### Step 2: Open Kitchen.unity
```
1. Open Unity
2. Open Assets/_Project/Scenes/Kitchen.unity
3. Save the scene (Ctrl+S) to ensure clean state
```

---

## 📋 Phase 2: Copy Essentials from Other Scenes

### Step 3: Copy Toilets from Kitchen_Articel_Quiz.unity

```
1. File → Open Scene → Kitchen_Articel_Quiz.unity
2. In Hierarchy, search for "Toilet"
3. You should see:
   - Der Toilet
   - Die Toilet  
   - Das Toilet
   - (Maybe parented under something like "Toilets" or "ArticleQuiz")

4. Select ALL THREE toilets (hold Ctrl)
5. Edit → Copy (Ctrl+C)

6. File → Open Scene → Kitchen.unity
7. In Hierarchy, right-click → Create Empty
   - Name: "ArticleQuiz_System"
   - Position: 0, 0, 0

8. Select ArticleQuiz_System
9. Edit → Paste (Ctrl+V)
10. Position toilets in a corner or designated area
    - Suggested: Near a wall, away from kitchen island
    - Make sure they're on the floor (y = 0)

11. Select each toilet, verify:
    - Has "QuizBasket" component
    - acceptedArticle = "Der", "Die", or "Das"
    - Has BoxCollider with IsTrigger = true
    - Has green/red light GameObjects assigned
```

### Step 4: Copy QuizGameManager UI from Kitchen_FindObject_Quiz.unity

```
1. File → Open Scene → Kitchen_FindObject_Quiz.unity

2. Find the Quiz UI (usually under Canvas or Managers):
   - Look for: "StartPanel", "GamePanel", "EndPanel"
   - Or find QuizGameManager component, see what panels it references

3. In Hierarchy, find the parent of these panels
   - Usually "Managers" or "QuizUI" GameObject
   - Or the Canvas containing quiz panels

4. Copy the ENTIRE Quiz UI structure:
   - Select the Canvas or parent GameObject
   - Edit → Copy (Ctrl+C)

5. Open Kitchen.unity
6. Paste (Ctrl+V)
7. Rename to "FindChallenge_UI"

8. Find QuizGameManager in this copied structure
   - Select it
   - In Inspector, verify ALL references:
     - startPanel: assigned?
     - gamePanel: assigned?
     - inputPanel: assigned?
     - endPanel: assigned?
     - questionText: assigned?
     - timerText: assigned?
     - feedbackText: assigned?
     - challengeMusicSource: assigned?

9. If any references are missing (show "None"):
   - Find the corresponding object in FindChallenge_UI
   - Drag it to the missing slot

10. Position FindChallenge_UI at 0, 0, 0
    - Or wherever your UI canvases are
```

### Step 5: Copy LeaderboardManager

```
1. Still in Kitchen_FindObject_Quiz.unity
2. Find "LeaderboardManager" GameObject
   - Usually under "Managers"
3. Copy it
4. Paste into Kitchen.unity
5. Parent it under FindChallenge_UI (or root)
```

---

## 📋 Phase 3: Create New Infrastructure

### Step 6: Create GameFlowController

```
1. In Kitchen.unity Hierarchy:
   - Right-click → Create Empty
   - Name: "GameFlowController"

2. Add Component → GameFlowController (from Scripts/Core)

3. Assign References in Inspector:
   
   Tablet Display:
   - Drag "Tablet Canvas" GameObject here
   
   Quiz Game Manager:
   - Drag the QuizGameManager from FindChallenge_UI
   
   Right Hand Scanner:
   - Find your right hand controller (under XR Origin)
   - Usually: XR Origin → Camera Offset → RightHand Controller
   - Drag it here
   
   Toilets Parent:
   - Drag "ArticleQuiz_System" GameObject
   
   Cleaning Spawn Area:
   - Right-click → Create Empty → "CleaningSpawnArea"
   - Position it where objects should gather (e.g., table center)
   - Drag it here
   
   Audio (optional):
   - Leave empty for now, or assign if you have audio sources
```

### Step 7: Create ArticleCleaningController

```
1. Right-click → Create Empty
   - Name: "ArticleCleaningController"

2. Add Component → ArticleCleaningController (from Scripts/Core)

3. Assign Toilets (if not auto-finding):
   - Find "Der Toilet" in ArticleQuiz_System
   - Drag to "Der Toilet" slot
   - Repeat for Die and Das

4. Leave Flush Particles empty (add later if you want)
```

---

## 📋 Phase 4: Setup TabletDisplay UI

### Step 8: Find Tablet Canvas

```
1. In Hierarchy, search: "Tablet"
2. Select "Tablet Canvas"

3. Verify it has "TabletDisplay" component
   - If not, add it
```

### Step 9: Create Find Challenge Prompt Panel

```
1. Right-click on Tablet Canvas → UI → Panel
   - Name: "FindChallengePromptPanel"
   - Position: 0, 0, 0 (center of tablet)
   - Size: 400 x 300

2. Right-click on FindChallengePromptPanel → UI → Text (TMP)
   - Text: "Challenge Available!"
   - Font size: 36
   - Center aligned
   - Position: top of panel

3. Right-click on FindChallengePromptPanel → UI → Text (TMP)
   - Text: "You've discovered 50% of items!\nReady for a timed challenge?"
   - Font size: 24
   - Center aligned
   - Position: middle

4. Right-click on FindChallengePromptPanel → UI → Button
   - Name: "AcceptButton"
   - Text: "Accept Challenge"
   - Position: bottom left
   
   On Click():
   - Drag Tablet Canvas
   - Select: TabletDisplay.OnAcceptFindChallenge()

5. Right-click on FindChallengePromptPanel → UI → Button
   - Name: "DeclineButton"
   - Text: "Later"
   - Position: bottom right
   
   On Click():
   - Drag Tablet Canvas
   - Select: TabletDisplay.OnDeclineFindChallenge()

6. DISABLE this panel (uncheck box in Inspector)
```

### Step 10: Create Cleaning Challenge Prompt Panel

```
1. Right-click on Tablet Canvas → UI → Panel
   - Name: "CleaningChallengePromptPanel"
   - Size: 400 x 300

2. Add Text (TMP):
   - "Kitchen Cleanup Time!"
   - "You've mastered 90% of items!\nTime to clean up by sorting objects."

3. Add Buttons:
   - "Start Cleaning" → OnAcceptCleaningChallenge()
   - "Later" → OnDeclineCleaningChallenge()

4. DISABLE this panel
```

### Step 11: Create Cleaning Mode Panel

```
1. Right-click on Tablet Canvas → UI → Panel
   - Name: "CleaningModePanel"
   - Size: 300 x 100 (smaller, just progress)

2. Right-click → UI → Text (TMP)
   - Name: "CleaningProgressText"
   - Text: "Cleaned: 0/10"
   - Font size: 32
   - Center aligned

3. DISABLE this panel
```

### Step 12: Create Message Text

```
1. Right-click on Tablet Canvas → UI → Text (TMP)
   - Name: "MessageText"
   - Text: "" (empty)
   - Font size: 28
   - Center aligned
   - Position: center of screen
   - Color: Yellow (visible)

2. DISABLE this
```

### Step 13: Assign to TabletDisplay

```
1. Select Tablet Canvas
2. Find TabletDisplay component
3. Drag panels to new fields:
   - Find Challenge Prompt Panel → findChallengePromptPanel
   - Cleaning Challenge Prompt Panel → cleaningChallengePromptPanel
   - Cleaning Mode Panel → cleaningModePanel
   - Cleaning Progress Text → cleaningProgressText (drag the Text object, not panel)
   - Message Text → messageText
```

---

## 📋 Phase 5: Verify WordItems

### Step 14: Check All Kitchen Objects

```
1. In Hierarchy, search: "WordItem"
   - Or look for kitchen objects (fridge, cup, etc.)

2. Select each one, verify:
   - Has "WordItem" component
   - Has XR Grab Interactable (or Simple Interactable)
   - objectID matches vocabulary.json

3. If any missing WordItem component:
   - Add it
   - It will auto-populate from object name
```

---

## 📋 Phase 6: Cleanup & Test

### Step 15: Remove Duplicate Managers

```
1. Check for duplicate singletons:
   - VocabularyManager (should be only one)
   - LeaderboardManager (should be only one)
   - QuizGameManager (only the one in FindChallenge_UI)

2. If duplicates found:
   - Delete the extra one
   - Keep the one with all references assigned
```

### Step 16: Save Scene

```
Ctrl + S
```

### Step 17: Test in Play Mode

```
1. Press Play
2. Scan a few objects normally (should work as before)
3. Check Console for errors

4. To test 50% trigger quickly:
   - In GameFlowController Inspector
   - Change "Find Challenge Threshold" to 0.1 (10%)
   - Scan one object
   - Should see prompt

5. Test Accept:
   - Should show Find Challenge UI
   - Timer should start
   - Find object should advance

6. Test Decline:
   - Should return to exploration

7. To test 90% trigger:
   - Change "Cleaning Threshold" to 0.2 (20%)
   - Complete Find Challenge first
   - Scan another object
   - Should see Cleaning prompt

8. Test Cleaning:
   - Objects should gather
   - Tablet shows "Cleaned: 0/10"
   - Try throwing wrong object to toilet
   - Try correct one - should disappear
```

---

## 📋 Phase 7: Update Build Settings

### Step 18: (Optional) Remove Standalone Scenes

```
If you want ONLY Kitchen.unity:

1. File → Build Settings
2. Remove:
   - Kitchen_FindObject_Quiz.unity
   - Kitchen_Articel_Quiz.unity
   - BasicScene.unity
3. Keep:
   - MainMenu.unity
   - Kitchen.unity
```

---

## 🔧 Troubleshooting

### Issue: "NullReferenceException: Object reference not set"
- Some reference not assigned
- Check GameFlowController and TabletDisplay inspector
- Look at Console for which line errors

### Issue: Find Challenge UI not showing
- QuizGameManager panels not assigned
- Or panels are disabled
- Check FindChallenge_UI is active in scene

### Issue: Objects not grabbable
- Missing XR Grab Interactable component
- Or interaction layers wrong

### Issue: Toilets not detecting objects
- Missing BoxCollider on toilets
- IsTrigger not checked
- Or QuizBasket component missing

---

## ✅ Final Checklist

- [ ] Toilets copied and positioned
- [ ] FindChallenge_UI copied and panels assigned
- [ ] LeaderboardManager present
- [ ] GameFlowController created and referenced
- [ ] ArticleCleaningController created
- [ ] TabletDisplay has 3 new panels assigned
- [ ] All WordItems have interactables
- [ ] No duplicate managers
- [ ] Scene saved
- [ ] Tested in Play Mode

---

## 🎉 Done!

Your Kitchen.unity now has:
- ✅ Full exploration mode
- ✅ Find Challenge at 50%
- ✅ Cleaning Challenge at 90%
- ✅ No scene loading during gameplay!
