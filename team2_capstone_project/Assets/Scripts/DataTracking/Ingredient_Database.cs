using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/IngredientDatabase", fileName = "Ingredient_Database")]
public class Ingredient_Database : ScriptableObject
{
  public List<Ingredient_Data> allIngredients;
  private Dictionary<IngredientType, Ingredient_Data> ingredientLookup;
  private Dictionary<IngredientType, Ingredient_Data> foragingLookup; // Foraging ingredients dictionary
  public List<Ingredient_Data> rawForagables;

  // Initialize the dictionary when the scriptable object is enabled
  private void OnEnable()
  {
    BuildDictionary();
  }

  private void BuildDictionary()
  {
    ingredientLookup = new Dictionary<IngredientType, Ingredient_Data>();
    foreach (var item in allIngredients)
    {
      if (item == null)
      {
        Debug.LogWarning("[Ingredient_Database]: Null item skipped in ingredient dictionary.");
        continue;
      }

      ingredientLookup[item.ingredientType] = item;
    }

    foragingLookup = new Dictionary<IngredientType, Ingredient_Data>();
    foreach (var item in rawForagables)
    {
      if (item == null)
      {
        Debug.LogWarning("[Ingredient_Database]: Null item skipped in foraging dictionary.");
        continue;
      }
      
      foragingLookup[item.ingredientType] = item;
    }
  }

  public Ingredient_Data GetIngredient(IngredientType type)
  {
    if (ingredientLookup.TryGetValue(type, out var data))
      return data;

    Debug.LogWarning($"[Ingredient_Database]: ingredient {type} not found in ingredient dictionary!");
    return null;
  }

  public Ingredient_Data GetForagableIngredient(IngredientType type)
  {
    if (foragingLookup.TryGetValue(type, out var data))
      return data;
    
    // no debug log warning here since there can be ingredients that are not in the foragable ingredient dictionary
    return null;
  }
}
