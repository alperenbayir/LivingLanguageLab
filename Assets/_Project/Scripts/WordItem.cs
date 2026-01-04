using UnityEngine;

public class WordItem : MonoBehaviour
{
    [Header("Debug Info (Read Only)")]
    public string objectID; 
    [HideInInspector] public string germanWord;

    void Start()
    {
        //  Get the name of this GameObject 
        string rawName = gameObject.name;

        //Clean the GameObject name
        //for multiple instance objects like cup-saucer (1)
        objectID = rawName.Split('(')[0].Trim();

        //Ask Manager for data
        if (VocabularyManager.Instance != null)
        {
            ItemData data = VocabularyManager.Instance.GetItem(objectID);

            if (data != null)
            {
                germanWord = data.german;
            
            }
            else
            {
                Debug.LogError(" JSON MISSING: Could not find ID '" + objectID + "' for object '" + gameObject.name + "'");
            }
        }
    }
}