using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPCs/NPC Data", fileName = "NewNPC")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    public NPCs npcID; // Unique enum identifier
    public string customerName;
    public Sprite overworldSprite;
    public bool datable = true;
    public string lore;

    [Header("Dialog Data")]
    public Character_Portrait_Data portraitData;

    [Header("Preferences")]
    public Dish_Data[] favoriteDishes;
    public Dish_Data[] neutralDishes;
    public Dish_Data[] dislikedDishes;

    // Enum to uniquely identify NPCs
    public enum NPCs
    {
        None,
        Phrog,
        Elf
        // Add all NPCs here
    }
}
