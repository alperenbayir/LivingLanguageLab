using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemData
{
    public string id;
    public string article_only;
    public string german;
    public string sentence;
    public bool canSort = true;
}

[System.Serializable]
public class ItemList
{
    public ItemData[] items;
}

public class VocabularyManager : MonoBehaviour
{
    public static VocabularyManager Instance;
    public Dictionary<string, ItemData> database = new Dictionary<string, ItemData>();

    void Awake()
    {
        Instance = this;
        LoadData();
    }

    void LoadData()
    {
        // Reads 'vocabulary.json' from the Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>("vocabulary");

        if (jsonFile != null)
        {
            ItemList data = JsonUtility.FromJson<ItemList>(jsonFile.text);

            foreach (ItemData item in data.items)
            {
                if (!database.ContainsKey(item.id))
                {
                    database.Add(item.id, item);
                }
            }
            Debug.Log("Database Loaded: " + database.Count + " words."); //Debug
        }
        else
        {
            Debug.LogError("Could not find 'vocabulary.json' in Resources folder!");
        }
    }

    public int GetTotalCount()
    {
        return database.Count;
    }

    public ItemData GetItem(string id)
    {
        if (database.ContainsKey(id))
            return database[id];

        return null;
    }

    /// <summary>
    /// Check if an item can be sorted in the article sorting challenge
    /// </summary>
    public bool CanSort(string id)
    {
        ItemData item = GetItem(id);
        return item != null && item.canSort;
    }
}