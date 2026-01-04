using UnityEngine;

public class AndroidTTS : MonoBehaviour
{
    private AndroidJavaObject ttsObject;
    private bool isInitialized = false;

    void Start()
    {
        // Bu kod sadece Android cihazda (Quest 3) calisir
#if UNITY_ANDROID && !UNITY_EDITOR
        InitializeAndroidTTS();
#endif
    }

    void InitializeAndroidTTS()
    {
        try
        {
            // Unity'nin calistigi Android aktivitesini al
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                // TTS Dinleyicisini (Listener) olustur
                TTSInitListener listener = new TTSInitListener();

                // Android TextToSpeech motorunu baslat
                ttsObject = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, listener);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("TTS Başlatma Hatası: " + e.Message);
        }
    }

    // --- DIŞARIDAN ÇAĞIRACAĞIMIZ FONKSİYON ---
    public void Speak(string text)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (ttsObject != null)
        {
            // Almanca (GERMAN) Ayarını Yap
            AndroidJavaClass localeClass = new AndroidJavaClass("java.util.Locale");
            AndroidJavaObject germanLocale = localeClass.GetStatic<AndroidJavaObject>("GERMAN");
            
            int result = ttsObject.Call<int>("setLanguage", germanLocale);
            
            if (result < 0) Debug.LogError("TTS: Almanca dili bu cihazda bulunamadi!");

            // Konusma Emri Ver (QUEUE_FLUSH = 0, yani oncekini sustur bunu soyle)
            // Parametreler: text, queueMode, bundle, utteranceId
            ttsObject.Call<int>("speak", text, 0, null, "id_unity_tts");
            
            Debug.Log("🗣️ Quest Konuşuyor: " + text);
        }
        else
        {
            Debug.LogWarning("TTS Henüz hazır değil veya başlatılamadı.");
        }
#else
        // PC'de test ederken hata vermesin, sadece log atsin
        Debug.Log("[PC SİMÜLASYONU] Konuşuluyor: " + text);
#endif
    }

    // Java tarafındaki "Hazır mı?" sinyalini dinleyen sınıf
    private class TTSInitListener : AndroidJavaProxy
    {
        public TTSInitListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }

        public void onInit(int status)
        {
            // status 0 = SUCCESS, -1 = ERROR
            if (status == 0)
            {
                Debug.Log("Android TTS Başarıyla Hazırlandı!");
            }
            else
            {
                Debug.LogError("Android TTS Hazırlanamadı. Hata Kodu: " + status);
            }
        }
    }
}