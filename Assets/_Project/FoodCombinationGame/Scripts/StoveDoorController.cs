using System.Collections;
using UnityEngine;

public class StoveDoorController : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Animator animator;
    [SerializeField] private string boolName = "IsOpen";

    [Header("Oven Logic")]
    [SerializeField] private OvenCrafter ovenCrafter;

    [Header("Timing")]
    [SerializeField] private float closeDuration = 0.6f;

    private bool isOpen;
    private Coroutine craftCoroutine;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (animator) isOpen = animator.GetBool(boolName);
    }

    // Diese Methode rufst du aus dem XR Simple Interactable Event auf
    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        if (animator) animator.SetBool(boolName, true);

        // Falls gerade ein "Craft nach Schließen" geplant war: abbrechen
        if (craftCoroutine != null)
        {
            StopCoroutine(craftCoroutine);
            craftCoroutine = null;
        }
    }

    public void CloseDoor()
    {
        if (!isOpen) return;

        isOpen = false;
        if (animator) animator.SetBool(boolName, false);

        // Crafting erst nach Ablauf der Close-Animation triggern
        if (craftCoroutine != null)
            StopCoroutine(craftCoroutine);

        craftCoroutine = StartCoroutine(CraftAfterDelay());
    }

    private IEnumerator CraftAfterDelay()
    {
        yield return new WaitForSeconds(closeDuration);
        ovenCrafter?.OnOvenClosed();
        craftCoroutine = null;
    }
}