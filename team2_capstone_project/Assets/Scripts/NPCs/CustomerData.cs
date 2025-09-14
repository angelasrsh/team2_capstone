using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cafe/Customer", fileName = "NewCustomer")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite dialoguePortrait;
    public Sprite overworldSprite;
    public bool datable = true;

    [Header("Preferences")]
    public Dish_Data[] favoriteDishes;
    public Dish_Data[] neutralDishes;
    public Dish_Data[] dislikedDishes;
}
