using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RightHandScanner : MonoBehaviour
{
    [Header("UI Connection")]
    public TabletDisplay tablet;
    public InputActionProperty scanButton; // The button you press (e.g., Trigger)

   
    [Header("Interaction Setup")]
    // We use XRBaseInteractor because 'NearFarInteractor' inherits from it.
    // This allows you to drag your Near-Far object directly into this slot.
    public XRBaseInteractor scannerInteractor;

    void Start()
    {
        // Auto-find logic
        if (scannerInteractor == null)
            scannerInteractor = GetComponent<XRBaseInteractor>();
    }

    void Update()
    {
        if (tablet.isProcessing) return; // lock the interaction if the tablet is active.


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
                    // --- BURASI DEĞİŞTİ ---

                    // 1. Önce Quiz Modunda mıyız diye kontrol et
                    if (QuizGameManager.Instance != null && QuizGameManager.Instance.IsGameActive)
                    {
                        // Quiz modundaysak cevabı QuizManager'a gönder
                        Debug.Log($"[Quiz] Cevap gönderiliyor: {item.objectID}");
                        QuizGameManager.Instance.SubmitAnswer(item.objectID);
                    }
                    // 2. Değilsek, eski öğrenme moduna (TabletDisplay) devam et
                    else if (tablet != null)
                    {
                        Debug.Log($"[Learn] Kelime gösteriliyor: {item.objectID}");
                        tablet.UpdateDisplay(item);
                    }

                    return; // İlk bulduğunda dur
                }
            }
        }
    }
}