using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Day_Plan_Manager : MonoBehaviour
{
    public static Day_Plan_Manager instance;

    public int customersPlannedForEvening { get; private set; } = 0;
    public List<Dish_Data.Dishes> todaySelectedDishes { get; private set; } = new();

    public static event System.Action<int> OnPlanUpdated;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SetPlan(List<Dish_Data.Dishes> selectedDishes, int customerCount)
    {
        if (Player_Progress.Instance != null && Player_Progress.Instance.InGameplayTutorial)
        {
            Debug.Log("[Day_Plan_Manager] Prevented a plan overwrite during tutorial.");
            return;
        }
        
        todaySelectedDishes = new List<Dish_Data.Dishes>(selectedDishes);
        customersPlannedForEvening = customerCount;

        Debug.Log($"Day plan set: {customerCount} customers, {selectedDishes.Count} dishes selected.");
        OnPlanUpdated?.Invoke(customerCount);
    }

    public void ClearPlan()
    {
        todaySelectedDishes.Clear();
        customersPlannedForEvening = 0;
    }
}
