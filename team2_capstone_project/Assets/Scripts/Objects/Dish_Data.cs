using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Dish", fileName = "NewDish")]
public class Dish_Data : Item_Data
{
    public Ingredient_Data[] ingredients;
    public bool isGoodDish;
    public float price;
    public Sprite dishSprite;
    [TextArea] public string recipeInstructions;
    public Dishes dishType;
    
    public enum Recipe
    {
        Stir,
        Chop,
        Toss
    }
    public Recipe recipe;
    
    public enum Dishes
    {
        Blinding_Stew
    }
}

[CreateAssetMenu(menuName = "Cafe/DishDatabase", fileName = "DishDatabase")]
public class Dish_Database_SO : ScriptableObject
{
    public List<Dish_Data> dishes;

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
        if (dishLookup.ContainsKey(dish))
            unlockedDishes.Add(dish);
        else
            Debug.LogWarning($"Dish {dish} does not exist in the database.");
    }

    public bool IsDishUnlocked(Dish_Data.Dishes dish)
    {
        return unlockedDishes.Contains(dish);
    }
}

// Alternative for tracking isGoodDish bool and other runtime stuff
// public class DishRuntimeData
// {
//     public DishData dishData;
//     public bool isGoodDish;
// }
