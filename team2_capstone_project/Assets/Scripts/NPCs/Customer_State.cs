using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Customer_State
{
    public string customerName;
    public int seatIndex;
    public string requestedDishName;
    public bool hasRequestedDish;
    public bool hasBeenServed;
}