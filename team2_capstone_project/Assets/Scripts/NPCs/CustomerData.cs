using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPCs/NPC Data", fileName = "NewNPC")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    public string customerName;
    public Sprite overworldSprite;
    public bool datable = true;

    [Header("Dialog Data")]
    public Character_Portrait_Data portraitData;

    [Header("Preferences")]
    public Dish_Data[] favoriteDishes;
    public Dish_Data[] neutralDishes;
    public Dish_Data[] dislikedDishes;
}
