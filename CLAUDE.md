# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Living Language Lab is a Unity 6 VR language learning game for Meta Quest 3. Players learn German vocabulary in an immersive kitchen environment through object scanning, AI-generated sentences (Ollama), and speech recognition (Wit.ai).

**See `AGENTS.md` for comprehensive project documentation** including technology stack, project structure, and implementation details.

## Key Development Commands

### Unity Editor
Open `LivingLanguageLab.sln` in Visual Studio or Rider. Unity 6 (6000.0.x) is required.

### Building for Quest 3
- Build target: Android (ARM64, IL2CPP)
- Minimum SDK: API 32
- Use Build Settings to create APK, then sideload via `adb install`

### External Dependencies
```bash
# AI sentence generation - must run during development
ollama serve
# Uses llama3.1:8b model at http://localhost:11434/api/generate
```

## Architecture Overview

### Scene Flow
`MainMenu` → `Kitchen` (main scene with 3-phase gameplay) → returns to `MainMenu`

The Kitchen scene implements a 3-phase progression:
1. **Exploration (0-50%)** - Scan objects with RightHandScanner, hear AI sentences
2. **Find Challenge (~15% intervals)** - Timer-based object hunt using QuizGameManager
3. **Cleaning Challenge (90%)** - Article sorting game via ArticleCleaningController

### Core Singletons
- `VocabularyManager` - Loads `vocabulary.json`, provides item lookup by ID
- `SentenceHistoryManager` (static) - Tracks discovered objects across gameplay
- `GameFlowController` - Orchestrates phase transitions and challenge triggers
- `GameSession` - Persists player data across scenes (`DontDestroyOnLoad`)

### VR Interaction Chain
`XR Origin` → `RightHandScanner` → scans `WordItem` components → `TabletDisplay` shows vocabulary info → `SentenceGenerator` fetches AI sentence from Ollama

### Critical Naming Convention
GameObject names **must match** the `id` field in `vocabulary.json`. The `WordItem.Start()` method auto-cleans instance suffixes (e.g., "cup-coffee (1)" → "cup-coffee").

## Data Files

### vocabulary.json (`Assets/_Project/Resources/vocabulary.json`)
```json
{
  "items": [{
    "id": "cup-coffee",       // Must match GameObject name
    "article_only": "Die",    // der/die/das
    "german": "Die Kaffeetasse",
    "english": "The Coffee Cup",
    "audioFileName": "cup-coffee"  // Maps to Resources/Audios/
  }]
}
```

### Audio Files
German pronunciations in `Assets/_Project/Resources/Audios/` must have filenames matching the `audioFileName` field.

## Code Patterns

### Standard Singleton
```csharp
public static GameFlowController Instance;
void Awake() { Instance = this; }
```

### VR Interactable Object Setup
Objects need:
1. `WordItem.cs` component with matching `objectID`
2. XR Grab Interactable or hover interactable
3. Entry in `vocabulary.json`

### Debug Prefixes
```csharp
Debug.Log("[GameFlow] ...");  // Phase transitions
Debug.Log("[History] ...");   // Discovery tracking
Debug.Log("[Quiz] ...");      // Challenge events
```

## External Integrations

### Ollama (AI Sentences)
- Endpoint: `POST http://localhost:11434/api/generate`
- Model: `llama3.1:8b`
- Code: `SentenceGenerator.cs`

### Wit.ai (Speech Recognition)
- Component: `AppVoiceExperience` on VoiceManager GameObject
- Config: `witConfiguration.asset` (contains API key - do not commit)
- Integration: `TabletDisplay.cs`

## Notes for Development
- Mixed comments exist (some Turkish legacy) - use English for new code
- Test in Editor with XR Device Simulator or via Quest Link
- LeaderboardManager uses PlayerPrefs for local score persistence
- The `RightHandScanner.CanScan` static property controls scanning globally
