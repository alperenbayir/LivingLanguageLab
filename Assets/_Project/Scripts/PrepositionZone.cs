using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PrepositionZone : MonoBehaviour
{
    public PrepositionType zoneType;
    public string targetTag = "Plate";

    [HideInInspector] public bool containsTarget;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        containsTarget = true;
        Debug.Log($"{name} ENTER: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        containsTarget = false;
        Debug.Log($"{name} EXIT: {other.name}");
    }
}

