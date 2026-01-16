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

        // Sýralama yap ve kaydet
        data.scores = data.scores.OrderBy(x => x.timeScore).Take(5).ToList(); // Ýlk 10'u tutalým
        SaveToDisk(data);
    }

    public LeaderboardData LoadScores()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            return JsonUtility.FromJson<LeaderboardData>(json);
        }
        return new LeaderboardData();
    }

    // --- YENÝ EKLENEN SÝLME FONKSÝYONLARI ---

    // Belirli bir sýradaki skoru siler
    public void DeleteScoreAtIndex(int index)
    {
        LeaderboardData data = LoadScores();

        if (index >= 0 && index < data.scores.Count)
        {
            data.scores.RemoveAt(index);
            SaveToDisk(data);
        }
    }

    // Tüm skorlarý siler
    public void ClearAllScores()
    {
        PlayerPrefs.DeleteKey(saveKey);
    }

    // Ortak kaydetme fonksiyonu
    private void SaveToDisk(LeaderboardData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }
}