using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/DishDatabase", fileName = "Dish_Database")]
public class Dish_Database : ScriptableObject
{
  public List<Dish_Data> dishes;
  public Dish_Data badDish; // Reference to a predefined bad dish
  private Dictionary<Dish_Data.Dishes, Dish_Data> dishLookup;

  // Initialize the dictionary when the scriptable object is enabled
  private void OnEnable()
  {
    BuildDictionary();
  }

  private void BuildDictionary()
  {
    dishLookup = new Dictionary<Dish_Data.Dishes, Dish_Data>();
    foreach (var dish in dishes)
    {
      dishLookup[dish.dishType] = dish;
    }
  }

  public Dish_Data GetDish(Dish_Data.Dishes dish)
  {
    if (dishLookup.TryGetValue(dish, out var data))
      return data;

    Debug.LogWarning($"Dish {dish} not found in database!");
    return null;
  }

  public Dish_Data GetBadDish()
  {
    if (badDish == null)
    {
      Debug.LogWarning("No bad dish found in the database!");
      return null;
    }

    return badDish;
  }

  public List<Dish_Data> GetAllDishes()
  {
    return new List<Dish_Data>(dishes); // return a copy to keep database safe
  }
}