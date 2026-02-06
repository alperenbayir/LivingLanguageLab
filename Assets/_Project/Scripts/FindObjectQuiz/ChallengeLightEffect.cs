using UnityEngine;

public class ChallengeLightEffect : MonoBehaviour
{
    private Light myLight;
    private float baseIntensity;
    private Quaternion startRotation;

    [Header("Pendulum Settings")]
    [Tooltip("0 = Left-Up, 1 = Left-Down, 2 = Right-Up, 3 = Right-Down")]
    public int pendulumIndex = 0;          // Which position in the pendulum chain
    public float sweepSpeed = 0.8f;        // How fast the pendulum swings
    public float sweepAngle = 50f;         // How far it swings (degrees)
    public bool waveMode = true;           // Lights move in a wave like connected pendulums

    [Header("Color & Intensity")]
    public float colorSpeed = 1.2f;        // Color change speed
    public float pulseSpeed = 2.5f;        // Intensity pulsing speed
    public float pulseAmount = 2f;         // How much intensity varies
    public float baseLightIntensity = 5f;  // Starting intensity

    void Start()
    {
        myLight = GetComponent<Light>();
        if (myLight != null)
        {
            myLight.intensity = baseLightIntensity;
            baseIntensity = baseLightIntensity;
            myLight.range = 20f;
        }
        
        startRotation = transform.rotation;
    }

    void Update()
    {
        if (myLight == null) return;

        float time = Time.time * sweepSpeed;
        float angle;

        if (waveMode)
        {
            // Connected pendulum wave effect
            // Each light is offset by 90 degrees (PI/2) in the sine wave
            // This creates a smooth wave: Left-Up -> Left-Down -> Right-Down -> Right-Up
            float phaseOffset = pendulumIndex * (Mathf.PI / 2f);
            angle = Mathf.Sin(time + phaseOffset) * sweepAngle;
        }
        else
        {
            // Simple synchronized swing
            angle = Mathf.Sin(time) * sweepAngle;
        }

        // Apply rotation relative to starting rotation
        transform.rotation = startRotation * Quaternion.Euler(0, angle, 0);

        // Cycle through rainbow colors with offset per light
        float hue = Mathf.Repeat(Time.time * colorSpeed + (pendulumIndex * 0.25f), 1f);
        myLight.color = Color.HSVToRGB(hue, 1f, 1f);

        // Pulse intensity
        float pulse = Mathf.Sin(Time.time * pulseSpeed + pendulumIndex) * 0.5f + 0.5f;
        myLight.intensity = baseIntensity + (pulse * pulseAmount);
    }
}
