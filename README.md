# Living Language Lab

A VR game for learning German vocabulary, built in Unity for the Meta Quest 3.
Fachprojekt in Human-AI/Computer Interaction at TU Dortmund.

## What it does

You stand in a virtual kitchen and scan objects with your hand. A scanned object becomes a
vocabulary item: the tablet you carry shows the German word with its article, plays the
pronunciation, and generates an example sentence. A counter tracks how many words you have found.

Once enough of the kitchen has been explored, challenges unlock:

- **Find-object quiz** — a word is given, you find and grab the matching object
- **Article quiz** — sort items into der / die / das baskets
- **Food combination game**
- **Preposition game** — place objects to practise positional prepositions

## Built with

- Unity 6 (6000.2.14f1)
- XR Interaction Toolkit, OpenXR, Oculus XR, XR Hands — tested on Meta Quest 3
- Meta XR Voice SDK (Wit.ai) for speech input
- Android text-to-speech for pronunciation playback
- Vocabulary and audio from `Assets/_Project/Resources/vocabulary.json` plus mp3 files
- LLM-generated example sentences, with history tracking so they do not repeat

## Structure

```
Assets/_Project/
├── Scenes/            Kitchen is the main scene; MainMenu, Cafe and one scene per challenge
├── Scripts/
│   ├── Core/          GameFlowController, ArticleCleaningController — phase progression
│   ├── DataControl/   GameSession
│   ├── FindObjectQuiz/  QuizGameManager, LeaderboardManager
│   ├── ArticelQuiz/   QuizManager, QuizBasket
│   ├── UI/            TabletMenuManager, LevelSelectionUI
│   └── RightHandScanner, VocabularyManager, WordItem, SentenceGenerator, TabletDisplay
└── Resources/         vocabulary.json and pronunciation audio
```

## Running it

Open the project in Unity 6 and build for Android with a Meta Quest as the target device.
It needs a headset — there is no desktop mode.

## Team

Built by three students over one semester. Individual contributions are listed in
`contribution.txt`.

## Status

A university project rather than a finished product. `GAME_DESIGN_DOCUMENT.txt` is a planning
document — parts of it describe intended features, not what is implemented.
