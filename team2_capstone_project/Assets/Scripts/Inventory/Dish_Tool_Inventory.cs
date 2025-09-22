using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Item_Stack (used by Inventory) for dishes since they can't stack (need a different stack limit)
/// </summary>
public class Dish_Tool_Stack : Item_Stack
{
    public override int stackLimit { get; set; } = 1;
}

/// <summary>
/// You can currently store dishes indefinitely. Probably not desired behavior,
/// but I'm leaving it for now. We probably change it semi-easily later.
/// 
/// </summary>
public class Dish_Tool_Inventory : Inventory
{
    public static Dish_Tool_Inventory Instance { get; protected set; }

    // How many stacks this inventory can have
    [field: System.NonSerialized] public override int InventorySizeLimit { get; set; } = 2;

    // Testing
    public Dish_Data TEST_DISH;

    new private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Create inventory stack structure to hold Dish_Tool_Stacks
        InitializeInventoryStacks<Dish_Tool_Stack>();
        updateInventory();

        AddResources(TEST_DISH, 1);
        AddResources(TEST_DISH, 3);
    }

    /// <summary>
    /// Overrides the base inventory AddResources function to use the Dish_Tool_Stack type instead.
    /// This is necessary to use the new StackLimit of 1 instead of the default of 20
    /// Also allows for other type differences later on.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    /// <returns>How many items were added</returns>
     public override int AddResources(Item_Data type, int count)
    {
        return addResourcesOfType<Dish_Tool_Stack>(type, count);
    }
}
