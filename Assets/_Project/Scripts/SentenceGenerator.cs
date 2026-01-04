using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class SentenceGenerator : MonoBehaviour
{
    private const string OLLAMA_URL = "http://localhost:11434/api/generate";
   

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

        // 2. Build Prompt (Compressed to single line for valid JSON)
        string promptText = $"Create a simple full German sentence (A1 level) in a kitchen context using '{item.germanWord}'. " +
                            $"Max 5 words. Use Subject-Verb-Object. " +
                            $"STRICTLY GERMAN ONLY. NO ENGLISH TRANSLATION. Output only the raw sentence. " +
                            $"Avoid: {avoidContext}";
        // 3. Prepare JSON
        string jsonBody = $@"
        {{
            ""model"": ""llama3.1:8b"",
            ""prompt"": ""{promptText}"",
            ""stream"": false
        }}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // 4. Send Request
        using (UnityWebRequest request = new UnityWebRequest(OLLAMA_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse Response
                OllamaResponse result = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);
                string finalSentence = result.response.Trim();

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