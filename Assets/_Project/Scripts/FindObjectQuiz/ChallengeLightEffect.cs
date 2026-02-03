using UnityEngine;

public class ChallengeLightEffect : MonoBehaviour
{
    private Light myLight;

    [Header("Ayarlar")]
    public float rotateSpeed = 100f; // Dönme hýzý
    public float colorSpeed = 0.5f;  // Renk deðiþtirme hýzý

    void Start()
    {
        myLight = GetComponent<Light>();
    }

    void Update()
    {
        // 1. Kendi etrafýnda döndür (Searchlight efekti)
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // 2. Rengi sürekli deðiþtir (Gökkuþaðý efekti)
        if (myLight != null)
        {
            // Zamanla deðiþen bir ton (Hue) deðeri üret (0 ile 1 arasý)
            float hue = Mathf.Repeat(Time.time * colorSpeed, 1f);
            // Bu tonu RGB rengine çevir
            myLight.color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }
}