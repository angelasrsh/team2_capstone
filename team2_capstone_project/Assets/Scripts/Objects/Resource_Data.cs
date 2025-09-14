using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Foraging/Resource", fileName = "NewResource")]
public class Ingredient_Data : Item_Data
{
    [Range(1.0f, 3.0f)] public int tier;
    public float price;

    [Header("Spawn Settings")]
    [Range(0f, 1f)] public float rarityWeight = 1f; // Higher = more likely
    public int minSpawn = 0;
    public int maxSpawn = 5;
}
