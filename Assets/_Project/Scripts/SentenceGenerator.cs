using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class SentenceGenerator : MonoBehaviour
{
    private const string OLLAMA_URL = "http://192.168.178.38:11434/api/generate";
    private const string MODEL = "gemma2:2b";

    void Start()
    {
        StartCoroutine(WarmUp());
    }

    private IEnumerator WarmUp()
    {
        string jsonBody = $"{{\"model\":\"{MODEL}\",\"prompt\":\"hi\",\"stream\":false}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(OLLAMA_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;
            yield return request.SendWebRequest();
            Debug.Log("[SentenceGenerator] Model warm-up complete.");
        }
    }

    // We use a "Callback" (Action<string>) to return the result when done
    public void RequestSentence(WordItem item, System.Action<string> onComplete)
    {
        StartCoroutine(GenerateRoutine(item, onComplete));
    }

    private IEnumerator GenerateRoutine(WordItem item, System.Action<string> onComplete)
    {
        // 1. Get History
        List<string> pastSentences = SentenceHistoryManager.GetHistory(item.objectID);

        string avoidContext = "";
        if (pastSentences.Count > 0)
        {
            string joinedHistory = string.Join(" | ", pastSentences);
            avoidContext = $" Vermeide diese S�tze: {joinedHistory}.";
        }

        // Strip article from german word (e.g. "Die Pflanze" -> "Pflanze") for cleaner prompt
        string wordOnly = item.germanWord.Contains(" ")
            ? item.germanWord.Substring(item.germanWord.IndexOf(' ') + 1)
            : item.germanWord;

        // 2. Build Prompt
        string promptText = $"Reply with exactly one complete German A1 sentence (subject + verb) using '{wordOnly}'. Plain text only, no markdown, no asterisks. Max 8 words.{avoidContext}";

        // 3. Prepare JSON
        string jsonBody = $"{{\"model\":\"{MODEL}\",\"prompt\":\"{promptText}\",\"stream\":false}}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // 4. Send Request
        using (UnityWebRequest request = new UnityWebRequest(OLLAMA_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse Response
                OllamaResponse result = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);
                // Take only the first line/sentence to strip any extra explanation the model adds
                string raw = result.response.Trim();
                string finalSentence = raw.Split('\n')[0].Trim()
                    .Replace("**", "").Replace("*", "").Replace("_", "").Trim();

                // Save to History
                SentenceHistoryManager.AddHistory(item.objectID, finalSentence);

                // RETURN result to the UI
                onComplete?.Invoke(finalSentence);
            }
            else
            {
                Debug.LogError("Ollama Error: " + request.error);
                onComplete?.Invoke("Connection Error");
            }
        }
    }
    [System.Serializable]
    public class OllamaResponse
    {
        public string response;
        public bool done;
        
    }
}