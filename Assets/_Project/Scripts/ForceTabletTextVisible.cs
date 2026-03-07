using UnityEngine;
using TMPro;

public class ForceTabletTextVisible : MonoBehaviour
{
    public TabletDisplay tablet;

    [Tooltip("Assign the Sentence Text (TextMeshProUGUI) from TabletDisplay here")]
    public TextMeshProUGUI sentenceText;

    [Tooltip("Optional: assign ScanLayout object")]
    public GameObject scanLayout;

    [Tooltip("Optional: assign IdleLayout object")]
    public GameObject idleLayout;

    private void Awake()
    {
        if (tablet == null) tablet = GetComponent<TabletDisplay>();
        if (tablet == null) tablet = FindObjectOfType<TabletDisplay>();
    }

    private void Start()
    {
        // Delay 1 frame so TabletDisplay.Start() runs first
        StartCoroutine(ForceNextFrame());
    }

    private System.Collections.IEnumerator ForceNextFrame()
    {
        yield return null;

        // Force scan mode + enable layout
        if (tablet != null)
            tablet.SetState(TabletDisplay.TabletMode.Scanning);

        if (idleLayout != null) idleLayout.SetActive(false);
        if (scanLayout != null) scanLayout.SetActive(true);

        // Force sentence text visible and readable
        if (sentenceText != null)
        {
            sentenceText.gameObject.SetActive(true);
            sentenceText.color = new Color(1, 1, 1, 1); // full alpha
            sentenceText.text = "Put the plate ON the microwave\n(Hold for 0.8s)";
        }
    }
}
