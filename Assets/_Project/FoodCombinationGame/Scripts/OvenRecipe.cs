using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Oven Recipe", fileName = "OvenRecipe")]
public class OvenRecipe : ScriptableObject
{
    [Tooltip("WordItem.objectID values required for this recipe (e.g. bread, tomato, cheese)")]
    public List<string> requiredIDs = new List<string>();

    [Tooltip("Prefab to spawn when recipe matches")]
    public GameObject resultPrefab;

    [Tooltip("ID of the result item (must match vocabulary.json id, e.g. 'pizza')")]
    public string resultID;

    [Tooltip("If true: consume ONLY the required items. If false: do not consume anything.")]
    public bool consumeRequiredItems = true;

    [Tooltip("If true: recipe also requires exact counts (e.g. 2x cheese). If false: only presence matters.")]
    public bool requireExactCounts = false;

    [Tooltip("Optional: If requireExactCounts is true, specify counts per ID (same length as requiredIDs)")]
    public List<int> requiredCounts = new List<int>();
}