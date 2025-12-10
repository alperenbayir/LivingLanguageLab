using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VocabularyItem : MonoBehaviour
{
    [Header("Settings")]
    public string wordID;
    public SmartLearningPanel panelController;
    public GameObject bubbleObject;

    private bool isHovering = false; 

    void Start()
    {
        if (bubbleObject) bubbleObject.SetActive(false);
    }

    void Update()
    {
      
        if (isHovering)
        {
          
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                OnActivate();
            }
        }
    }

    // HOVER ENTER 
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        isHovering = true;
        if (bubbleObject) bubbleObject.SetActive(true);
    }

    // HOVER EXIT 
    public void OnHoverExit(HoverExitEventArgs args)
    {
        isHovering = false;
        if (bubbleObject) bubbleObject.SetActive(false);
    }

    // SELECT ENTERED 
    public void OnSelectEnter(SelectEnterEventArgs args)
    {
        OnActivate();
    }

    // ACTIVATE Panel
    public void OnActivate()
    {
        if (panelController != null)
        {
            panelController.ShowWord(wordID);
            
            if (bubbleObject) bubbleObject.SetActive(false);
        }
    }
}