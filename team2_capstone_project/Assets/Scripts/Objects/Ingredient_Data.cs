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

  public Sprite[] CutIngredientImages;
  public List<Dish_Data> usedInDishes;
  public List<Ingredient_Requirement> makesIngredient; // e.g. 1 bone used to make bone broth
}

// public class CutLine
// {
//     public Transform startMarker;
//     public Transform endMarker;
//     public bool cutCompleted;
// }
