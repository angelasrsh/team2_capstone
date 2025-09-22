using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/DishDatabase", fileName = "Dish_Database")]
public class Dish_Database : ScriptableObject
{
  public List<Dish_Data> dishes;
  public Dish_Data badDish; // Reference to a predefined bad dish
  public event System.Action OnDishUnlocked; // Event to notify when a dish is unlocked
  private Dictionary<Dish_Data.Dishes, Dish_Data> dishLookup;
  private HashSet<Dish_Data.Dishes> unlockedDishes = new HashSet<Dish_Data.Dishes>();

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

  public void UnlockDish(Dish_Data.Dishes dish)
  {
    if (dishLookup.ContainsKey(dish) && unlockedDishes.Add(dish))
      OnDishUnlocked?.Invoke(); // Notify subscribers
    else
      Debug.LogWarning($"Dish {dish} does not exist in the database.");
  }

  public bool IsDishUnlocked(Dish_Data.Dishes dish)
  {
    return unlockedDishes.Contains(dish);
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
    public HashSet<Dish_Data.Dishes> GetUnlockedDishes()
  {
    return new HashSet<Dish_Data.Dishes>(unlockedDishes);
  }
}