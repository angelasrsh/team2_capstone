using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Dish", fileName = "NewDish")]
public class Dish_Data : Item_Data
{
    public Resource_Data[] ingredients;
    public bool isGoodDish;
    public float price;

    public enum Recipe
    {
        Stir,
        Chop,
        Toss
    }
    public Recipe recipe;
}

// Alternative for tracking isGoodDish bool and other runtime stuff
// public class DishRuntimeData
// {
//     public DishData dishData;
//     public bool isGoodDish;
// }
