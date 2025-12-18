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

  // --- Mix Minigame Settings ---
  public float mixDuration = 5f;      // how long the minigame lasts
  // public float targetClicksPerSecond = 8f; // player must average this CPS
  // public float tolerance = 2f;        // +- range allowed (optional)
  public int totalClicks = 30;


  public enum Dishes
  {
    Failed_Dish, // Generic failed dish if no recipe matches
    Mc_Dragons_Burger,
    Blinding_Stew,
    Honey_Jelly_Drink,
    Charcuterie_Board,
    Boba_Milk_Drink,
    Honey_Glazed_Eleonoras,
    Hot_Phoenix_Wings_With_Rice,
    None
  }

  public List<Ingredient_Requirement> ingredientQuantities;
}

public enum Recipe
{
  None,
  Cauldron,
  Chop,
  Fry,
  Combine,
  Mix
}

// Alternative for tracking isGoodDish bool and other runtime stuff
// public class DishRuntimeData
// {
//     public DishData dishData;
//     public bool isGoodDish;
// }
