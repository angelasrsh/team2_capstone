using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scriptable object base class for items that can be held in inventory
/// Requires a name, amount, and sprite
/// </summary>
public class Item_Data : ScriptableObject
{
    public string ItemName;
    public int ItemAmount;
    public Sprite ItemSprite;


}
