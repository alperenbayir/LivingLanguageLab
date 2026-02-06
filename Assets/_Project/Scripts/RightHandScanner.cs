using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RightHandScanner : MonoBehaviour
{
    public static RightHandScanner Instance;
    
    [Header("UI Connection")]
    public TabletDisplay tablet;
    public InputActionProperty scanButton; // The button you press (e.g., Trigger)

   
    [Header("Interaction Setup")]
    // We use XRbaseInteractor because 'NearFarInteractor' inherits from it.
    // This allows you to drag your Near-Far object directly into this slot.
    public XRBaseInteractor scannerInteractor;
    
    // Static flag to disable scanning during challenges
    public static bool CanScan = true;
    
    // Hover tracking
    private WordItem currentHoveredItem = null;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        // Auto-find logic
        if (scannerInteractor == null)
            scannerInteractor = GetComponent<XRBaseInteractor>();
    }

    void Update()
    {
        if (!CanScan) 
        {
            ClearHoverHighlight();
            return; // Scanner locked during prompts/end panel
        }
        if (tablet.isProcessing) 
        {
            ClearHoverHighlight();
            return; // lock the interaction if the tablet is active.
        }
        
        // Handle hover highlighting
        UpdateHoverHighlight();


        if (scanButton.action.WasPressedThisFrame())
            {
               
                ScanCurrentHover();
            }
        
    }

    void ScanCurrentHover()
    {
        if (scannerInteractor == null) return;

        List<IXRHoverInteractable> hoverList = scannerInteractor.interactablesHovered;

        if (hoverList.Count > 0)
        {
            foreach (var target in hoverList)
            {
                WordItem item = target.transform.GetComponent<WordItem>();
                if (item == null) item = target.transform.GetComponentInParent<WordItem>();

                if (item != null)
                {
                    // --- CHECK 1: Are we in EndPanel? If so, dismiss it and DON'T scan ---
                    if (QuizGameManager.Instance != null && QuizGameManager.Instance.TryDismissEndPanel())
                    {
                        // EndPanel was dismissed by this scan
                        // Now show this item on tablet (first scan after ending)
                        if (tablet != null)
                        {
                            tablet.UpdateDisplay(item);
                        }
                        return;
                    }
                    
                    // --- CHECK 2: Are we in Find Challenge? ---
                    if (QuizGameManager.Instance != null && QuizGameManager.Instance.IsGameActive)
                    {
                        // Quiz mode - submit answer
                        Debug.Log($"[Quiz] Submitting: {item.objectID}");
                        QuizGameManager.Instance.SubmitAnswer(item.objectID);
                    }
                    // --- CHECK 3: Normal exploration mode ---
                    else if (tablet != null)
                    {
                        Debug.Log($"[Learn] Displaying: {item.objectID}");
                        tablet.UpdateDisplay(item);
                    }

                    return; // Stop after first found
                }
            }
        }
    }
    
    /// <summary>
    /// Updates hover highlight on objects
    /// </summary>
    void UpdateHoverHighlight()
    {
        if (scannerInteractor == null) return;
        
        List<IXRHoverInteractable> hoverList = scannerInteractor.interactablesHovered;
        WordItem newHoveredItem = null;
        
        // Find first WordItem being hovered
        if (hoverList.Count > 0)
        {
            foreach (var target in hoverList)
            {
                WordItem item = target.transform.GetComponent<WordItem>();
                if (item == null) item = target.transform.GetComponentInParent<WordItem>();
                
                if (item != null)
                {
                    newHoveredItem = item;
                    break;
                }
            }
        }
        
        // Handle hover change
        if (newHoveredItem != currentHoveredItem)
        {
            // Clear old highlight
            if (currentHoveredItem != null)
            {
                currentHoveredItem.OnHoverEnd();
            }
            
            // Set new highlight
            currentHoveredItem = newHoveredItem;
            if (currentHoveredItem != null)
            {
                currentHoveredItem.OnHoverStart();
            }
        }
    }
    
    /// <summary>
    /// Clears any active hover highlight
    /// </summary>
    void ClearHoverHighlight()
    {
        if (currentHoveredItem != null)
        {
            currentHoveredItem.OnHoverEnd();
            currentHoveredItem = null;
        }
    }
    
}