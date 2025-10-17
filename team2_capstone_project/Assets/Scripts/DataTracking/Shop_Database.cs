using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/ShopDatabase", fileName = "Shop_Database")]
public class Shop_Database : ScriptableObject
{
  public List<Ingredient_Data> allItems; // List of items that can be sold in shop
  private Dictionary<IngredientType, Ingredient_Data> itemLookup;

  // Initialize the dictionary when the scriptable object is enabled
  private void OnEnable()
  {
    BuildDictionary();
  }

  private void BuildDictionary()
  {
    itemLookup = new Dictionary<IngredientType, Ingredient_Data>();
    foreach (var item in allItems)
    {
      if (item == null)
      {
        Debug.LogWarning("[Shop_Database]: Null item skipped.");
        continue;
      }

      itemLookup[item.ingredientType] = item;
    }
  }

  public Ingredient_Data GetItem(IngredientType type)
  {
    if (itemLookup.TryGetValue(type, out var data))
      return data;

    Debug.LogWarning($"[Shop_Database]: ingredient {type} not found in database!");
    return null;
  }
}
