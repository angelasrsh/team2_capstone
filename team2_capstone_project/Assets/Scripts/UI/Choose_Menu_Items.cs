using System.Collections.Generic;
using UnityEngine;

public class Choose_Menu_Items : MonoBehaviour
{
    public static Choose_Menu_Items instance;
    private List<Dish_Data.Dishes> dishesSelected = new List<Dish_Data.Dishes>();

    public static event System.Action<List<Dish_Data.Dishes>> OnDailyMenuSelected;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddDish(Dish_Data.Dishes dishType)
    {
        if (!dishesSelected.Contains(dishType))
        {
            dishesSelected.Add(dishType);
            Debug.Log(dishType + " added. Total dishes: " + dishesSelected.Count);
        }
        else
        {
            Debug.Log(dishType + " is already selected.");
        }
    }

    public void RemoveDish(Dish_Data.Dishes dishType)
    {
        if (dishesSelected.Contains(dishType))
        {
            dishesSelected.Remove(dishType);
            Debug.Log(dishType + " removed. Total dishes: " + dishesSelected.Count);
        }
        else
        {
            Debug.Log(dishType + " is not in the selected list.");
        }
    }

    public List<Dish_Data.Dishes> GetSelectedDishes() => dishesSelected;

    public bool HasSelectedDishes() => dishesSelected.Count > 0;

    // Call this when player confirms their menu
    public void NotifyMenuConfirmed()
    {
        OnDailyMenuSelected?.Invoke(new List<Dish_Data.Dishes>(dishesSelected));
    }
}
