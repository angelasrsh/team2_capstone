using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Dish", fileName = "NewDish")]
public class DishData : ScriptableObject
{
    public string dishName;
    public Sprite dishSprite;
    public ResourceData[] ingredients;
    public float price;
}
