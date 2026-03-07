using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OvenCrafter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OvenVolumeTracker ovenVolume;
    [SerializeField] private Transform spawnPoint;

    [Header("Recipes")]
    [SerializeField] private OvenRecipeDatabase recipeDatabase;

    [Header("Behavior")]
    [SerializeField] private bool allowMultipleCrafts = false; // wenn false: craftet nur, wenn noch kein Result im Ofen liegt

    private GameObject currentResultInstance;

    public void OnOvenClosed()
    {
        if (!ovenVolume || !spawnPoint || !recipeDatabase)
        {
            Debug.LogError("OvenCrafter: Missing references (ovenVolume/spawnPoint/recipeDatabase).");
            return;
        }

        ovenVolume.CleanupNulls();

        // Falls schon Ergebnis drin und wir wollen nicht mehrfach craften:
        if (!allowMultipleCrafts && currentResultInstance != null)
            return;

        // IDs der Items im Ofen sammeln
        var items = ovenVolume.ItemsInOven.Where(x => x != null).ToList();
        var ids = items.Select(i => i.objectID).ToList();

        // Passendes Rezept suchen
        OvenRecipe match = FindMatchingRecipe(ids, recipeDatabase.recipes);
        if (match == null)
            return;

        // Items ggf. konsumieren
        if (match.consumeRequiredItems)
        {
            ConsumeRequiredItems(items, match);
        }

        // Result spawnen
        if (match.resultPrefab != null)
        {
            currentResultInstance = Instantiate(match.resultPrefab, spawnPoint.position, spawnPoint.rotation);

            // Optional: Ergebnis bleibt erstmal liegen (nicht rumkullern)
            var rb = currentResultInstance.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            if (match.resultID == "pizza")
                FoodComboUIManager.Instance?.OnPizzaCrafted();
            else
                FoodComboUIManager.Instance?.ShowScanPrompt();
        }
        else
        {
            Debug.LogWarning($"OvenCrafter: Recipe '{match.name}' has no resultPrefab.");
        }
    }

    private OvenRecipe FindMatchingRecipe(List<string> idsInOven, List<OvenRecipe> recipes)
    {
        // Optional: längere/komplexere Rezepte zuerst matchen
        foreach (var recipe in recipes.OrderByDescending(r => r.requiredIDs.Count))
        {
            if (recipe == null || recipe.requiredIDs == null || recipe.requiredIDs.Count == 0)
                continue;

            if (!recipe.requireExactCounts)
            {
                // Presence-based: alle required IDs müssen vorkommen (mind. einmal)
                bool ok = recipe.requiredIDs.All(req => idsInOven.Contains(req));
                if (ok) return recipe;
            }
            else
            {
                // Count-based: benötigte Anzahl pro ID
                if (recipe.requiredCounts == null || recipe.requiredCounts.Count != recipe.requiredIDs.Count)
                    continue;

                bool ok = true;
                for (int i = 0; i < recipe.requiredIDs.Count; i++)
                {
                    string id = recipe.requiredIDs[i];
                    int need = recipe.requiredCounts[i];
                    int have = idsInOven.Count(x => x == id);
                    if (have < need) { ok = false; break; }
                }
                if (ok) return recipe;
            }
        }

        return null;
    }

    private void ConsumeRequiredItems(List<WordItem> itemsInOven, OvenRecipe recipe)
    {
        if (!recipe.requireExactCounts)
        {
            // zerstöre je requiredID genau 1 passendes Item (oder alle? -> hier: mindestens 1 pro ID)
            foreach (var req in recipe.requiredIDs.Distinct())
            {
                var toDestroy = itemsInOven.FirstOrDefault(w => w != null && w.objectID == req);
                if (toDestroy != null)
                    Destroy(toDestroy.gameObject);
            }
        }
        else
        {
            // zerstöre exakt benötigte Mengen
            for (int i = 0; i < recipe.requiredIDs.Count; i++)
            {
                string id = recipe.requiredIDs[i];
                int need = recipe.requiredCounts[i];

                var candidates = itemsInOven.Where(w => w != null && w.objectID == id).Take(need).ToList();
                foreach (var c in candidates)
                    Destroy(c.gameObject);
            }
        }
    }
}