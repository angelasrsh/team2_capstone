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

  public enum Recipe
  {
    None,
    Stir,
    Chop,
    Toss
  }
  public Recipe recipe;

  public enum Dishes
  {
    Failed_Dish, // Generic failed dish if no recipe matches
    Blinding_Stew
  }

  public List<Ingredient_Requirement> ingredientQuantities;
}

[System.Serializable]
public class Ingredient_Requirement
{
  public Ingredient_Data ingredient; 
  public int amountRequired;      
}

// Alternative for tracking isGoodDish bool and other runtime stuff
// public class DishRuntimeData
// {
//     public DishData dishData;
//     public bool isGoodDish;
// }
