# Living Language Lab - Agent Documentation

## Project Overview

**Living Language Lab** is a VR language learning game built with Unity, targeting Meta Quest 3 (Android VR). The game teaches German vocabulary through an immersive kitchen environment where players interact with objects, learn their German names and articles (der/die/das), practice pronunciation with AI feedback, and complete gamified challenges.

### Key Features
- **VR Kitchen Environment**: Explore a fully interactive kitchen with 50+ vocabulary items
- **AI-Powered Learning**: Uses local LLM (Ollama) to generate contextual German sentences
- **Speech Recognition**: Wit.ai integration for pronunciation practice and feedback
- **3-Phase Game Flow**: Exploration → Find Challenge (50%) → Cleaning Challenge (90%)
- **Article Learning**: Emphasis on German grammatical articles through "Viscera Cleanup Detail" style sorting game

---

## Technology Stack

| Category | Technology | Version |
|----------|------------|---------|
| Game Engine | Unity | 6000.0.x (Unity 6) |
| Render Pipeline | URP (Universal Render Pipeline) | 17.2.0 |
| VR Framework | XR Interaction Toolkit | 3.2.2 |
| VR Platform | OpenXR + Oculus XR Plugin | 1.16.0 / 4.5.2 |
| Hand Tracking | XR Hands | 1.7.1 |
| Voice SDK | Meta Voice SDK (Wit.ai) | 81.0.0 |
| AI Backend | Ollama (local LLM) | llama3.1:8b |
| Scripting Backend | IL2CPP | - |
| Target Platform | Android (Quest 3) | API 32+ |

### Unity Packages (Key Dependencies)
```json
{
  "com.meta.xr.sdk.voice": "81.0.0",
  "com.unity.inputsystem": "1.16.0",
  "com.unity.render-pipelines.universal": "17.2.0",
  "com.unity.xr.hands": "1.7.1",
  "com.unity.xr.interaction.toolkit": "3.2.2",
  "com.unity.xr.management": "4.5.3",
  "com.unity.xr.oculus": "4.5.2",
  "com.unity.xr.openxr": "1.16.0"
}
```

---

## Project Structure

```
Assets/
├── _Project/                      # Main project code and assets
│   ├── Scenes/                    # Game scenes
│   │   ├── MainMenu.unity         # Entry point with level selection
│   │   ├── Kitchen.unity          # Main kitchen exploration scene
│   │   ├── Cafe.unity             # Alternative environment (WIP)
│   │   ├── Kitchen_Articel_Quiz.unity    # Article sorting mini-game
│   │   └── Kitchen_FindObject_Quiz.unity # Find object challenge scene
│   ├── Scripts/                   # C# source code
│   │   ├── Core/                  # Core game systems
│   │   │   ├── GameFlowController.cs      # 3-phase game orchestration
│   │   │   └── ArticleCleaningController.cs # Viscera-style cleaning game
│   │   ├── DataControl/           # Data persistence
│   │   │   └── GameSession.cs             # Singleton for cross-scene data
│   │   ├── UI/                    # UI controllers
│   │   │   ├── LevelSelectionUI.cs        # Main menu level selector
│   │   │   └── TabletMenuManager.cs       # In-game tablet UI
│   │   ├── ArticelQuiz/           # Article quiz components
│   │   │   ├── QuizBasket.cs              # Der/Die/Das baskets
│   │   │   └── QuizManager.cs             # Answer validation
│   │   ├── FindObjectQuiz/        # Find object challenge
│   │   │   ├── QuizGameManager.cs         # Timer-based hunt game
│   │   │   └── LeaderboardManager.cs      # Local score persistence
│   │   ├── VocabularyManager.cs   # JSON vocabulary loader
│   │   ├── WordItem.cs            # Component for interactable objects
│   │   ├── TabletDisplay.cs       # Main tablet UI controller
│   │   ├── RightHandScanner.cs    # VR interaction handler
│   │   ├── SentenceGenerator.cs   # Ollama LLM integration
│   │   ├── SentenceHistoryManager.cs      # Sentence deduplication
│   │   └── AndroidTTS.cs          # Quest 3 native TTS
│   ├── Resources/                 # Runtime-loaded assets
│   │   ├── vocabulary.json        # 50-item German vocabulary database
│   │   └── Audios/                # German pronunciation audio files
│   └── Settings/                  # URP and project settings
├── Samples/                       # Unity sample packages
│   ├── XR Hands/                  # Hand tracking samples
│   └── XR Interaction Toolkit/    # Starter assets and demos
├── ThirdParty/                    # Third-party assets
│   ├── VRTemplateAssets/          # Unity VR Template scripts
│   ├── kitchen-environment/       # Free kitchen assets
│   └── low-poly-food/             # Food item assets
└── Simple Voice Chat/             # Voice chat utility scripts
```

---

## Core Architecture

### Manager Pattern (Singletons)
The project uses a singleton-based manager architecture for core systems:

| Manager | Purpose | Location |
|---------|---------|----------|
| `VocabularyManager` | Loads vocabulary.json, provides item lookup | Scene singleton |
| `SentenceHistoryManager` | Tracks discovered items, stores sentence history | Static class |
| `GameFlowController` | Orchestrates 3-phase gameplay | Scene singleton |
| `GameSession` | Persists player selections across scenes | `DontDestroyOnLoad` |
| `QuizGameManager` | Manages find-object challenge game flow | Scene singleton |
| `QuizManager` | Validates article quiz answers | Scene singleton |
| `ArticleCleaningController` | Handles cleaning challenge mechanics | Scene singleton |
| `LeaderboardManager` | Saves/loads high scores using PlayerPrefs | Scene singleton |

### Vocabulary Data Structure
Vocabulary is stored in `Assets/_Project/Resources/vocabulary.json`:

```json
{
  "items": [
    {
      "id": "cup-coffee",
      "article_only": "Die",
      "german": "Die Kaffeetasse",
      "english": "The Coffee Cup",
      "audioFileName": "cup-coffee",
      "sentence": ""
    }
  ]
}
```

**Important**: Object GameObject names must match the `id` field (without `(Clone)` or instance numbers).

### 3-Phase Game Flow
1. **Exploration Phase (0-50%)**: Player freely scans objects, learns vocabulary, hears AI-generated sentences
2. **Find Challenge (50% trigger)**: Timed hunt to find 5 previously discovered objects
3. **Cleaning Challenge (90% trigger)**: "Viscera Cleanup Detail" style game - grab objects and throw into correct Der/Die/Das toilets

---

## Build Configuration

### Target Platform Settings
- **Platform**: Android
- **Minimum SDK**: 32 (Android 12L)
- **Target Architecture**: ARM64
- **Scripting Backend**: IL2CPP
- **Target Devices**: Meta Quest 3

### Build Scenes (in order)
1. `MainMenu` - Entry point
2. `Kitchen` - Main exploration scene
3. `Cafe` - Alternative environment
4. `Kitchen_Articel_Quiz` - Article sorting mini-game

### Scripting Define Symbols (Android)
- `USE_INPUT_SYSTEM_POSE_CONTROL`
- `UNITY_POST_PROCESSING_STACK_V2`
- `USE_STICK_CONTROL_THUMBSTICKS`

---

## External Dependencies & Setup

### Ollama (Required for AI Sentences)
The game requires a local Ollama instance running on the development machine:

```bash
# Install Ollama from https://ollama.ai
# Pull the required model:
ollama pull llama3.1:8b

# Start Ollama server (defaults to http://localhost:11434)
ollama serve
```

**Code Reference**: `SentenceGenerator.cs` uses `UnityWebRequest` to POST to `http://localhost:11434/api/generate`

### Wit.ai (Required for Speech Recognition)
The game uses Meta's Voice SDK (Wit.ai) for speech-to-text:

1. Create a Wit.ai app at https://wit.ai
2. Configure `witConfiguration.asset` in Unity
3. VoiceManager GameObject must exist in scene with `AppVoiceExperience` component

**Code Reference**: `TabletDisplay.cs` handles Wit.ai integration for pronunciation scoring

### Audio Files
German audio pronunciations must be placed in:
```
Assets/_Project/Resources/Audios/
├── cup-coffee.mp3
├── der-fisch.mp3
└── ... (matching vocabulary.json audioFileName field)
```

---

## Code Conventions

### Language & Comments
- **Primary language**: English (code, documentation)
- **Mixed comments**: Some scripts have Turkish comments (legacy)
- **New code**: Use English comments only

### Naming Conventions
- **Classes**: PascalCase (e.g., `GameFlowController`)
- **Methods**: PascalCase (e.g., `OnObjectDiscovered`)
- **Private fields**: camelCase with underscore prefix (e.g., `_nextTargetRatio`)
- **Public fields**: camelCase (e.g., `findChallengeThreshold`)
- **Constants**: UPPER_SNAKE_CASE or PascalCase

### Key Code Patterns

#### Singleton Pattern (Standard)
```csharp
public static GameFlowController Instance;

void Awake()
{
    Instance = this;
}
```

#### Cross-Scene Persistence
```csharp
void Awake()
{
    if (Instance != null) {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

#### VR Interaction Handler
```csharp
public class RightHandScanner : MonoBehaviour
{
    public XRBaseInteractor scannerInteractor;
    
    void ScanCurrentHover()
    {
        List<IXRHoverInteractable> hoverList = scannerInteractor.interactablesHovered;
        foreach (var target in hoverList)
        {
            WordItem item = target.transform.GetComponent<WordItem>();
            // ...
        }
    }
}
```

---

## Testing & Development

### Testing Scenarios
1. **Editor Testing**: Works in Unity Editor with XR Device Simulator
2. **Quest Link**: Test with Quest 3 via Link cable
3. **Build Testing**: Create Android APK and test on-device

### Common Issues

#### Ollama Connection Failed
- Ensure Ollama is running: `ollama serve`
- Check firewall settings for port 11434
- For Quest standalone: Ollama must be accessible via network or replaced with cloud API

#### Wit.ai Not Working
- Verify `VoiceManager` GameObject exists in scene
- Check `AppVoiceExperience` component configuration
- Ensure microphone permission is granted on Quest

#### Missing Audio
- Verify audio files exist in `Resources/Audios/`
- Check `audioFileName` in vocabulary.json matches filename
- Ensure AudioSource is assigned in scene

### Debug Logging
The project uses Unity's Debug.Log with prefixes for filtering:
```csharp
Debug.Log("[GameFlow] Starting Find Challenge");
Debug.Log("[History] New Discovery: " + objectId);
Debug.Log("[Quiz] Answer submitted");
```

---

## Security Considerations

### API Keys
- **Wit.ai Server Access Token**: Stored in `witConfiguration.asset` (DO NOT commit to public repos)
- **Ollama**: Local-only by default, no authentication

### Data Storage
- Leaderboard data stored in `PlayerPrefs` (local device only)
- No network communication except to Ollama (localhost) and Wit.ai

---

## Important Implementation Notes

### WordItem Component
All interactable vocabulary objects must have:
1. `WordItem.cs` component attached
2. GameObject name matching `id` in vocabulary.json
3. XR Grab Interactable or similar for VR interaction

### GameObject Naming
Names are automatically cleaned in `WordItem.Start()`:
```csharp
// "cup-coffee (1)" becomes "cup-coffee"
objectID = rawName.Split('(')[0].Trim();
```

### VR Rig Requirements
- Must use XR Origin with Camera Offset
- Right hand controller needs `NearFarInteractor` or `XRBaseInteractor`
- UI Canvas must use World Space render mode with Tracked Device Graphic Raycaster

---

## Resources & References

- **Game Design Document**: `GAME_DESIGN_DOCUMENT.txt` (comprehensive feature plan)
- **Vocabulary Database**: `Assets/_Project/Resources/vocabulary.json`
- **Build Settings**: `ProjectSettings/EditorBuildSettings.asset`
- **Unity Version**: Project uses Unity 6000.0.x (Unity 6)
