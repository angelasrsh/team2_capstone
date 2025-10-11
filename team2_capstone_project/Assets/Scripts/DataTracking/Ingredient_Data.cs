using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Foraging/Ingredient", fileName = "NewIngredient")]
public class Ingredient_Data : Item_Data
{
  [Range(1.0f, 3.0f)] public int tier;
  public float price;

  [Header("Spawn Settings")]
  [Range(0f, 1f)] public float rarityWeight = 1f;
  public int minSpawn = 0;
  public int maxSpawn = 5;

  [Header("Recipe Equivalents")]
  // mainly used to count processed variants (e.g. chopped) as their base ingredient
  // used to tracking ingredient requirements for dishes in Journal_Menu
  public List<Ingredient_Data> countsAs;
  public IngredientType ingredientType;
  public string description;
  public Sprite[] CutIngredientImages;
  public List<Dish_Data> usedInDishes;
  public List<Ingredient_Requirement> makesIngredient; // e.g. 1 bone used to make bone broth
  public List<Ingredient_Requirement> ingredientsNeeded; // ingredients needed to make this ingredient
}

public enum IngredientType
{
  Null,
  Milk,
  Cheese,
  Uncut_Fogshroom,
  Uncut_Fermented_Eye,
  Slime_Gelatin,
  Bone_Broth,
  Bone,
  Cut_Fermented_Eye,
  Cut_Fogshroom,
  Water,
  Uncooked_Patty,
  Cooked_Patty,
  Bread,
  Uncut_Mandrake,
  Cut_Mandrake,
  French_Fries,
  Honey,
  Oil
}

// public class CutLine
// {
//     public Transform startMarker;
//     public Transform endMarker;
//     public bool cutCompleted;
// }
