# VR German Learning - Kitchen Scene Merge

## Quick Context
VR game teaching German articles via kitchen exploration. Player scans WordItems (linked to JSON vocabulary) → sees sentences on Tablet UI → plays ArticleQuiz sorting game at 60% progress. Currently split across Kitchen scene and others scenes you can see on /Scenes

## File Map (What Matters)

### READ & ANALYZE (Scripts):
- **Scripts/ArticleQuiz/** - The separate sorting game logic (toilets, scoring)
- **Scripts/DataControl/** - Progress tracking, JSON vocab loader, WordItem data
- **Scripts/FindObject.../** - Raycasting/interaction for scanning objects
- **Scripts/UI/** - TabletDisplay, wrist UI, progress bars
- **WordItem.cs** - Core component on kitchen objects (article + word data)
- **SentenceG... / SentenceH...** - LLM sentence generation
- **Vocabulary...** - JSON parser

### READ & ANALYZE (Scenes - Kimi reads YAML):
- **Scenes/Kitchen.unity** - Main exploration (YAML text, analyze hierarchy)
- **Scenes/BasicScene.unity** - Likely has ArticleQuiz GameObjects (analyze what to migrate)
- **Scenes/Cafe.unity** - Reference for object setup (if needed)

### IGNORE (Clutter):
- texture/ (all image files)
- SampleScenes/ 
- Scenes/Kitchen_Fin... (baked lighting scenes - huge binary noise)
- Library/, Build/, Logs/
- Any .fbx, .png, .wav in root

## Scene Combination Strategy

**PROBLEM**: ArticleQuiz logic sits in BasicScene, exploration in KitchenScene.

**SOLUTION** (Hybrid approach):
1. **Kimi analyzes** both scene files to identify GameObject hierarchies
2. **You manually merge** in Unity (safer than automated)
3. **Kimi writes** the state management scripts to connect them

### Specific Migration Checklist (You do this):
1. Open **BasicScene.unity** (source)
2. Find GameObjects: `ArticleQuizManager`, `Toilets` (Der/Die/Das), `ObjectSpawner`, `QuizTable`
3. Copy these objects (Ctrl+C)
4. Open **Kitchen.unity** (target)
5. Paste as children of new empty `ArticleQuiz_System` GameObject
6. Delete BasicScene from build settings later

## Detailed TODO

1. **Scene Analysis Scripts**: 
   - Create `Editor/SceneAnalyzer.cs` that parses Kitchen.unity and BasicScene.unity YAML to list all GameObjects with their components
   - Output: "BasicScene has: ArticleManager(MonoBehaviour), Toilet_Red(Collider)... Kitchen has: WordItem(25x), Table(Canvas)..."

2. **Flow State Machine**:
   - Create `GameFlowController.cs` (Scripts/Core/)
   - States: EXPLORING → CHECKPOINT → ARTICLE_GAME → COMPLETE
   - At 60%/70%/80%/90%: Trigger UI popup (use existing TabletDisplay)
   - At 100%: Force `ArticleQuiz_System` activation (the objects you pasted), disable exploration raycaster

3. **Integration Points**:
   - Modify `ProgressTracker` (DataControl) to call `GameFlowController.OnScanComplete()`
   - Modify `ArticleQuiz` (ArticleQuiz folder) to accept external trigger (currently likely auto-starts on scene load)
   - Ensure `TabletDisplay` can switch modes: SentenceDisplay ↔ QuizPrompt

4. **Cleanup**:
   - After merge, BasicScene.unity should be deprecated (remove from BuildSettings)
   - Ensure only one XR Origin exists (in Kitchen, delete duplicate from BasicScene paste if any)

## Critical Instructions for Kimi
- When reading scenes: Look for `GameObject:` YAML blocks with `m_Name:` (object names) and `MonoBehaviour:` components
- Don't suggest editing Library/ or ProjectSettings/ unless specific input bindings needed
- Keep existing WordItem.cs intact - it's the data bridge
- ArticleQuiz logic should not know about exploration; GameFlowController orchestrates both
- Ask me questions if there are some missing parts due to you can not see the hiearchie and gameobjects clear enough