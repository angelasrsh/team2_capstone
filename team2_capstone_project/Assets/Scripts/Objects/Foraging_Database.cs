using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Foraging/ForagingDatabase", fileName = "Foraging_Database")]
public class Foraging_Database : ScriptableObject
{
    [Tooltip("All possible foraging items that can be discovered.")]
    public List<Ingredient_Data> foragingItems;

    public event System.Action OnForagingItemUnlocked; // Notify UI updates

    private Dictionary<string, Ingredient_Data> foragingLookup;
    private HashSet<string> unlockedItems = new HashSet<string>();

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

    public Ingredient_Data GetItem(string itemName)
    {
        if (foragingLookup.TryGetValue(itemName, out var data))
            return data;

        Debug.LogWarning($"Foraging item {itemName} not found in database!");
        return null;
    }

    public void UnlockItem(string itemName)
    {
        if (foragingLookup.ContainsKey(itemName) && unlockedItems.Add(itemName))
        {
            OnForagingItemUnlocked?.Invoke(); // notify listeners
        }
        else
        {
            Debug.LogWarning($"Item {itemName} does not exist in the foraging database.");
        }
    }

    public bool IsItemUnlocked(string itemName)
    {
        return unlockedItems.Contains(itemName);
    }

    public HashSet<string> GetUnlockedItems()
    {
        return new HashSet<string>(unlockedItems);
    }
}
