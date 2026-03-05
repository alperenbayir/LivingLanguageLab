using System.Collections.Generic;
using UnityEngine;

public class OvenVolumeTracker : MonoBehaviour
{
    // Alle WordItems, die gerade im Ofen sind
    private readonly HashSet<WordItem> itemsInOven = new HashSet<WordItem>();

    public IReadOnlyCollection<WordItem> ItemsInOven => itemsInOven;

    private void OnTriggerEnter(Collider other)
    {
        // Falls Collider Kindobjekt ist, holen wir WordItem am Parent
        var wordItem = other.GetComponentInParent<WordItem>();
        if (wordItem != null)
            itemsInOven.Add(wordItem);
    }

    private void OnTriggerExit(Collider other)
    {
        var wordItem = other.GetComponentInParent<WordItem>();
        if (wordItem != null)
            itemsInOven.Remove(wordItem);
    }

    // Optional: falls Items zerstört werden, damit keine "dead references" bleiben
    public void CleanupNulls()
    {
        itemsInOven.RemoveWhere(x => x == null);
    }
}