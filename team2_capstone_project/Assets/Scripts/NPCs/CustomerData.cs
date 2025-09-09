using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cafe/Customer", fileName = "NewCustomer")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite dialoguePortrait;
    public Sprite overworldSprite;

    [Header("Preferences")]
    public string[] favoriteDishes; 
    public string[] neutralDishes;
    public string[] dislikedDishes;

    [Header("Personality")]
    public float generosity = 1f;    // multiplier for tip
}
