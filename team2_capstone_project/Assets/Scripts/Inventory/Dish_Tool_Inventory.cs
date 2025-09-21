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

    // Testing
    public Dish_Data dish;

    // End Testing
    new private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Do this instead of calling the base.awake() so that we can 
        if (InventoryStacks == null)
            InventoryStacks = new Dish_Tool_Stack[InventorySizeLimit];
        else if (InventoryStacks.Length != InventorySizeLimit)
        {
            Item_Stack[] temp = InventoryStacks; // not super efficient but oh well
            InventoryStacks = new Item_Stack[InventorySizeLimit];

            for (int i = 0; i < temp.Length; i++) // copy over elements
            {
                InventoryStacks[i] = temp[i];
            }
        }


        updateInventory();


        // Testing
        AddResources(dish, 2);
        AddResources(dish, 1);
        // End testing
    }

}
