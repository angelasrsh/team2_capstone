using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Foraging/ForagingDatabase", fileName = "Foraging_Database")]
public class Foraging_Database : ScriptableObject
{
    [Tooltip("All possible foraging items that can be discovered.")]
    public List<Ingredient_Data> foragingItems;
    private Dictionary<string, Ingredient_Data> foragingLookup;

    // Initialize when ScriptableObject loads
    private void OnEnable()
    {
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        foragingLookup = new Dictionary<string, Ingredient_Data>();
        foreach (var item in foragingItems)
        {
            if (item == null || string.IsNullOrEmpty(item.Name))
            {
                Debug.LogWarning("Foraging_Database: Null or unnamed item skipped.");
                continue;
            }

            foragingLookup[item.Name] = item;
        }
    }

    public Ingredient_Data GetItem(string Name)
    {
        if (foragingLookup.TryGetValue(Name, out var data))
            return data;

        Debug.LogWarning($"Foraging item {Name} not found in database!");
        return null;
    }
}
