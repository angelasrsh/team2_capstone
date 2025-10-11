using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/IngredientDatabase", fileName = "Ingredient_Database")]
public class Ingredient_Database : ScriptableObject
{
  public List<Ingredient_Data> allIngredients;
  private Dictionary<IngredientType, Ingredient_Data> ingredientLookup;
  public List<Ingredient_Data> rawForagables = new List<Ingredient_Data>();

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
        Debug.LogWarning("[Ingredient_Database]: Null item skipped.");
        continue;
      }

      ingredientLookup[item.ingredientType] = item;
    }
  }

  public Ingredient_Data GetIngredient(IngredientType type)
  {
    if (ingredientLookup.TryGetValue(type, out var data))
      return data;

    Debug.LogWarning($"[Ingredient_Database]: ingredient {type} not found in database!");
    return null;
  }
}
