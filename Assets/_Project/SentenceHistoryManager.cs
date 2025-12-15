using System.Collections.Generic;
using UnityEngine;


public static class SentenceHistoryManager
{
    private static Dictionary<string, List<string>> history = new Dictionary<string, List<string>>();
    private const int MaxHistoryCount = 4; // llm gets bloated with sentences

    public static List<string> GetHistory(string objectId)
    {
        if (history.ContainsKey(objectId))
            return history[objectId];

        return new List<string>();
    }

    public static void AddHistory(string objectId, string newSentence)
    {
        if (!history.ContainsKey(objectId))
            history[objectId] = new List<string>();

        history[objectId].Add(newSentence);
       
        if (history[objectId].Count > MaxHistoryCount)
            history[objectId].RemoveAt(0);

        Debug.Log($"[History] Added sentence for '{objectId}'.");
    }

    // Returns how many unique objects have been discovered
    public static int GetDiscoveredCount()
    {
        return history.Count;
    }
}