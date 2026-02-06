# Living Language Lab - Final Presentation

## Course Information
- **Course:** Human AI/Computer Interaction Fachprojekt
- **Team:** 4 members
- **Core Concept:** Situated Learning in VR - "Look, Learn, Memorize"
- **Goal:** Replace abstract vocabulary lists with tangible 3D experiences

---

## Slide 1: Title
**Living Language Lab**
*Immersive German Language Learning in VR*

- Team names, course, date
- Meta Quest 3 + Unity logos

---

## Slide 2: Problem & Motivation

**The Problem:**
- Traditional language learning = memorizing abstract word lists
- Words feel disconnected from real-world context
- Low engagement and retention

**Research Question:**
> "How can VR create situated learning experiences that connect vocabulary to tangible objects?"

*Visual: Flashcards vs. VR kitchen comparison*

---

## Slide 3: Our Solution - Situated Learning

**Framework: Look, Learn, Memorize**

1. **Look** - See real 3D objects in context
2. **Learn** - Hear pronunciation, read AI sentences
3. **Memorize** - Practice through interactive games

*Diagram: See Object → Learn Word → Practice → Test*

---

## Slide 4: System Architecture

**Technology Stack:**
| Component | Technology | Purpose |
|-----------|------------|---------|
| VR Platform | Meta Quest 3 | Immersive display + hand tracking |
| Game Engine | Unity 6 | 3D environment + physics |
| Interaction | XR Interaction Toolkit | Grabbing, pointing, triggers |
| AI Sentences | Ollama (llama3.1:8b) | Contextual sentence generation |
| Speech | Wit.ai | Pronunciation scoring |

*Simple diagram: Player → VR Headset → Unity → AI Services*

---

## Slide 5: Game 1 - Object Scanning & Discovery

**Learning Goal:** Vocabulary acquisition + article memorization

**How It Works:**
- Point hand scanner at any kitchen object
- Tablet displays German word with article (der/die/das)
- AI generates a unique contextual sentence
- Hear native pronunciation
- Practice speaking with accuracy feedback

**Key Features:**
- 65+ kitchen vocabulary items
- AI never repeats sentences (tracks history)
- Speech recognition scores pronunciation

*Screenshot: Tablet showing "Die Kaffeetasse" with AI sentence*

---

## Slide 6: Game 2 - Find Object Challenge

**Learning Goal:** Active recall + spatial memory

**How It Works:**
- Voice announces: "Find the [object]!"
- Timer counts down
- Player searches the kitchen
- Scanning correct object = success

**Game Design:**
- Triggers automatically at 15% discovery intervals
- 5 objects per challenge round
- Audio + visual feedback on success

*Screenshot: Challenge UI with timer and object name*

---

## Slide 7: Game 3 - Article Sorting (Der/Die/Das)

**Learning Goal:** German grammatical gender mastery

**How It Works:**
- Three baskets labeled: DER, DIE, DAS
- Grab objects and drop in correct basket
- Green light = Correct!
- Red light = Try again

**Game Design:**
- Triggers at 80% discovery progress
- Immediate feedback reinforces learning
- Objects teleport back for retry on wrong answer

*Screenshot: Three baskets with colored feedback lights*

---

## Slide 8: Game 4 - Preposition Game

**Learning Goal:** Spatial prepositions (auf, in, unter, neben, vor, hinter)

**How It Works:**
- Tablet shows instruction: "Place the plate UNDER the microwave"
- Player grabs the plate
- Places it in the correct position relative to reference object
- System detects correct placement automatically

**Technical Approach (Simplified):**
- Invisible "zones" around the reference object (on top, inside, under, beside, etc.)
- System checks which zone contains the plate when released
- Must stay in correct zone briefly to confirm
- Green feedback on success → next challenge

**Key Features:**
- Randomized preposition order (no repeats until all used)
- Real-time position tracking
- Clear visual feedback

*Screenshot: Plate near microwave with instruction tablet*

---

## Slide 9: Game 5 - Food Combo Game (Oven Crafting)

**Learning Goal:** Compound words + contextual vocabulary

**The Concept:**
> "Just like real cooking, only meaningful combinations create results"

**How It Works:**
1. Grab ingredients from the kitchen
2. Place them inside the oven
3. Close the oven door
4. If valid recipe → new item appears!
5. Learn the German compound word

**Technical Approach (Simplified):**
- Each ingredient has a unique ID (e.g., "bread", "cheese", "tomato")
- Oven has a sensor zone that tracks what's inside
- Closing the door triggers the "cook" action
- System checks ingredients against recipe database
- Valid combo → ingredients consumed, result spawns

**Recipe System:**
- Recipes stored as data files (not hardcoded)
- Easy to add new combinations without coding
- Example: Bread + Cheese + Tomato = Pizza ("Die Pizza")

**Key Insight:**
- Invalid combinations do nothing (mirrors real language learning)
- Only meaningful word combinations make sense

*Screenshot: Oven with ingredients, result item spawning*

---

## Slide 10: Demo Highlights

**Quick Walkthrough (~2 minutes):**
1. Scan a coffee cup → See "Die Kaffeetasse"
2. Hear AI sentence + pronunciation
3. Try speaking the word
4. Article sorting challenge
5. (Optional) Oven crafting demo

*Have backup video ready*

---

## Slide 11: Technical Challenges & Solutions

| Challenge | Solution |
|-----------|----------|
| Objects falling through surfaces | Proper collider setup + physics layers |
| Grab conflicts with scanning | Disable scanner when holding objects |
| AI repeating sentences | Pass sentence history to LLM prompt |
| Wrong answer frustration | Gentle feedback + teleport to retry |
| Detecting "inside" vs "on top" | Separate trigger zones per position |

---

## Slide 12: Lessons Learned

**VR Development:**
- Physics requires careful collider configuration
- Multiple interaction types need state management
- Hand tracking adds immersion but needs fallbacks

**Learning Design:**
- Progressive difficulty keeps players engaged
- Immediate feedback is essential
- Physical interaction creates memorable associations
- AI personalization increases engagement

**Team Collaboration:**
- Clear component ownership prevents conflicts
- Version control essential for Unity projects

---

## Slide 13: Limitations & Future Work

**Current Limitations:**
- Single environment (Kitchen only)
- Requires local Ollama server
- Quest 3 exclusive
- ~65 vocabulary items

**Future Possibilities:**
- New environments: Cafe, Supermarket, Office, Bedroom
- Language levels: A1 → A2 → B1 → B2
- Multiplayer collaborative learning
- Learning analytics dashboard
- Additional languages (French, Spanish, etc.)
- Cloud-based AI (no local server needed)

---

## Slide 14: Conclusion

**Key Achievements:**
- 5 distinct game modes for vocabulary reinforcement
- AI-powered personalized sentences
- Speech recognition for pronunciation practice
- Physical interaction with grammar concepts

**Core Insight:**
> "Words become objects, objects become memories"

VR transforms abstract vocabulary into tangible, interactive experiences that stick.

---

## Slide 15: Q&A

**Questions?**

- Demo available after presentation
- [GitHub link if applicable]
- Contact information

---

## Demo Checklist

Before presentation:
- [ ] Ollama server running (`ollama serve`)
- [ ] Quest 3 charged and connected
- [ ] All 5 game modes tested
- [ ] Backup video recorded and ready
- [ ] Key objects accessible (cup, plate, food items)
- [ ] Article baskets visible and working
- [ ] Oven crafting ingredients prepared

---

## Visual Assets Needed

1. **Screenshots:**
   - Tablet display with German word
   - Find challenge UI with timer
   - Article sorting baskets with lights
   - Preposition game setup
   - Oven with ingredients

2. **Diagrams:**
   - System architecture (simple)
   - Learning flow diagram

3. **Video:**
   - 2-minute demo walkthrough (backup)

4. **Optional:**
   - Team photo
   - Before/after comparison graphic
