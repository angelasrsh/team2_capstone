using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Day_Summary_Data
{
    public Day_Turnover_Manager.WeekDay currentDay;
    public Day_Turnover_Manager.WeekDay nextDay;
    public Dictionary<Dish_Data, int> dishesServed;
    public Dictionary<string, int> customersServed;
    public int totalCurrencyEarned;
}