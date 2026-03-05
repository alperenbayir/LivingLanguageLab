using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Oven Recipe Database", fileName = "OvenRecipeDatabase")]
public class OvenRecipeDatabase : ScriptableObject
{
    public List<OvenRecipe> recipes = new List<OvenRecipe>();
}