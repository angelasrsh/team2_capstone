using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Day_Turnover_Manager : MonoBehaviour
{
    public static Day_Turnover_Manager Instance;

    public enum WeekDay { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
    public WeekDay CurrentDay { get; private set; } = WeekDay.Monday;

    // Dict for tracking dishes + customers served each day
    private Dictionary<Dish_Data, int> dishesServed = new Dictionary<Dish_Data, int>();
    private Dictionary<string, int> customersServed = new Dictionary<string, int>();
    private int totalCurrencyEarned = 0;

    public static event System.Action<Day_Summary_Data> OnDayEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void RecordDishServed(Dish_Data dish, int currencyGained, string customerName)
    {
        if (dish != null)
        {
            if (!dishesServed.ContainsKey(dish)) dishesServed[dish] = 0;
            dishesServed[dish]++;
        }

        if (!string.IsNullOrEmpty(customerName))
        {
            if (!customersServed.ContainsKey(customerName)) customersServed[customerName] = 0;
            customersServed[customerName]++;
        }

        totalCurrencyEarned += currencyGained;
    }

    public void EndDay()
    {
        var nextDay = (WeekDay)(((int)CurrentDay + 1) % 7);

        var summary = new Day_Summary_Data
        {
            currentDay = CurrentDay,   // use the existing field
            nextDay = nextDay,
            dishesServed = new Dictionary<Dish_Data, int>(dishesServed),
            customersServed = new Dictionary<string, int>(customersServed),
            totalCurrencyEarned = totalCurrencyEarned
        };

        OnDayEnded?.Invoke(summary);

        // Reset for next day
        dishesServed.Clear();
        customersServed.Clear();
        totalCurrencyEarned = 0;
        CurrentDay = nextDay;
    }
}

