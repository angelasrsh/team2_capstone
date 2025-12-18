using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Customer_State
{
    public string customerName;
    public int seatIndex;
    // public string requestedDishName;
    public Dish_Data.Dishes requestedDishType;
    public bool hasRequestedDish;
    public bool hasBeenServed;
    public bool isTutorialCustomer;
}