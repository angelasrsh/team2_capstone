using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Dish", fileName = "NewDish")]
public class Dish_Data : Item_Data
{
  public bool isGoodDish;
  public float price;
  // [TextArea] public string recipeInstructions;
  public Sprite recipeImage;
  public Dishes dishType;
  public Recipe recipe;

  public enum Dishes
  {
    Failed_Dish, // Generic failed dish if no recipe matches
    Mc_Dragons_Burger,
    Blinding_Stew,
    Honey_Jelly_Drink,
    Charcuterie_Board,
    Boba_Milk_Drink
  }

  public List<Ingredient_Requirement> ingredientQuantities;
}

public enum Recipe
{
  None,
  Cauldron,
  Chop,
  Fry,
  Combine
}

// Alternative for tracking isGoodDish bool and other runtime stuff
// public class DishRuntimeData
// {
//     public DishData dishData;
//     public bool isGoodDish;
// }
