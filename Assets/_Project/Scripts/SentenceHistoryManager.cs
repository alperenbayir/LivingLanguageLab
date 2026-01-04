using System.Collections.Generic;
using UnityEngine;


public static class SentenceHistoryManager
{

    //Sentence history for the LLM
    private static Dictionary<string, List<string>> sentenceHistory = new Dictionary<string, List<string>>();
    private const int MaxHistoryCount = 4; //Too much sentences confuse the LLM

    //ObjectID history for counting and tracking (SFX and counter label)
    private static HashSet<string> discoveredIDs = new HashSet<string>();


    public static List<string> GetHistory(string objectId)
    {
        if (sentenceHistory.ContainsKey(objectId))
            return sentenceHistory[objectId];

        return new List<string>();
    }

    public static void MarkAsDiscovered(string objectId)
    {
        if (string.IsNullOrEmpty(objectId)) return;

        if (!discoveredIDs.Contains(objectId))
        {
            discoveredIDs.Add(objectId);
            Debug.Log($"[History] New Discovery: {objectId}");
        }
    }

    public static bool IsDiscovered(string objectId)
    {
        return discoveredIDs.Contains(objectId);
    }

    public static int GetDiscoveredCount()
    {
        return discoveredIDs.Count;
    }

    //This function is for LLM
    public static void AddHistory(string objectId, string newSentence)
    {
        if (!sentenceHistory.ContainsKey(objectId))
            sentenceHistory[objectId] = new List<string>();

        sentenceHistory[objectId].Add(newSentence);
       
        if (sentenceHistory[objectId].Count > MaxHistoryCount)
            sentenceHistory[objectId].RemoveAt(0);

        Debug.Log($"[History] Added sentence for '{objectId}'.");
    }


    // NOT USED YET (PREPARED FOR THE NEW SCENE)
    public static void ResetHistory()
    {
        discoveredIDs.Clear();
        sentenceHistory.Clear();
    }


}