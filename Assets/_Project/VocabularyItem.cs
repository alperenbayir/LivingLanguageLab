using UnityEngine;
// If you are using the XR Interaction Toolkit, this line may be important,
// otherwise it might cause errors.
// But for event-based systems, we only need public functions,
// so we keep it simple.

public class VocabularyItem : MonoBehaviour
{
    public string wordID; // We will write "apple" in the Inspector
    public SmartLearningPanel panelController; // Drag the Canvas object here

    // This will run when the user looks at the object
    public void OnHoverEnter()
    {
        if (panelController != null)
            panelController.ShowWord(wordID);
    }

    // This will run when the user stops looking at the object
    public void OnHoverExit()
    {
        if (panelController != null)
            panelController.HidePanel();
    }
}
