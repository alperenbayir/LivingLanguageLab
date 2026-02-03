using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public float timeScore;
}

[System.Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> scores = new List<ScoreEntry>();
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;
    private string saveKey = "KitchenQuizLeaderboard";

    void Awake()
    {
        Instance = this;
    }

    public void SaveScore(string name, float time)
    {
        LeaderboardData data = LoadScores();
        data.scores.Add(new ScoreEntry { playerName = name, timeScore = time });

        // S�ralama yap ve kaydet
        data.scores = data.scores.OrderBy(x => x.timeScore).Take(5).ToList(); // �lk 10'u tutal�m
        SaveToDisk(data);
    }

    public LeaderboardData LoadScores()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            Debug.Log($"[LeaderboardManager] Loaded JSON: {json}");
            return JsonUtility.FromJson<LeaderboardData>(json);
        }
        Debug.Log("[LeaderboardManager] No saved scores found");
        return new LeaderboardData();
    }

    // --- YEN� EKLENEN S�LME FONKS�YONLARI ---

    // Belirli bir s�radaki skoru siler
    public void DeleteScoreAtIndex(int index)
    {
        LeaderboardData data = LoadScores();

        if (index >= 0 && index < data.scores.Count)
        {
            data.scores.RemoveAt(index);
            SaveToDisk(data);
        }
    }

    // T�m skorlar� siler
    public void ClearAllScores()
    {
        Debug.Log("[LeaderboardManager] Clearing all scores");
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
        Debug.Log("[LeaderboardManager] Scores cleared");
    }

    // Ortak kaydetme fonksiyonu
    private void SaveToDisk(LeaderboardData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }
}