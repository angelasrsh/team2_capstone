using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Foraging/Ingredient", fileName = "NewIngredient")]
public class Ingredient_Data : Item_Data
{
    [Range(1.0f, 3.0f)] public int tier;
    public float price;

    [Header("Spawn Settings")]
    [Range(0f, 1f)] public float rarityWeight = 1f; // Higher = more likely
    public int minSpawn = 0;
    public int maxSpawn = 5;

}

// public class CutLine
// {
//     public Transform startMarker;
//     public Transform endMarker;
//     public bool cutCompleted;
// }
