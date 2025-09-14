using System.Collections;
using System.Collections.Generic;
using System;
// using Unity.Android.Types;
using Unity.VisualScripting;
// using UnityEditor.iOS;
using UnityEngine;
using Grimoire;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem;
using System.Linq;

[System.Serializable]
public class Item_Stack
{
    public Item_Data resource;
    public int amount;
    public int stackLimit = 20;
}

// [System.Serializable]
// public class Dish_Stack
// {
//     public Dish_Data dish;
//     public int amount;
// }


// Gives the player a collection of items of a fixed size
public class Inventory : MonoBehaviour
{
    public int InventorySizeLimit = 12;
    
    [field: SerializeField]
    private Item_Stack[] InventoryStacks;

    /// <summary>
    /// Initialize Inventory to be size InventorySizeLimit
    /// </summary>
    protected void Awake()
    {
        InventoryStacks = new Item_Stack[InventorySizeLimit];
        PrintInventory();
    }
    
    /// <summary>
    /// Add resources and update inventory.
    /// </summary>
    /// <param name="type"> The type of item to add </param> 
    /// <param name="count"> How many of the item to add</param> 
    /// <returns> The number of items actually added </returns>
    public int AddResources(Item_Data type, int count)
    {
         // Error-checking
        if (count < 0)
            Debug.LogError("[Invtry] Cannot add negative amount"); // not tested

        // Track the amount of resources we still need to add
        int amtLeftToAdd = count;

        // Check if there is a slot with the same type and add if not full
        foreach (Item_Stack istack in InventoryStacks)
        {
            // Add as much as we can to existing stacks
            if (istack != null && istack.resource == type && istack.amount < istack.stackLimit)
            {
                int amtToAdd = Math.Min(istack.stackLimit - istack.amount, amtLeftToAdd);
                istack.amount += amtToAdd;
                amtLeftToAdd -= amtToAdd;
            }
        }
        // We were not able to add all items to existing slots, so check if we can start a new stack
        // These are two separate loops because we don't assume slots will be filled in order
        for (int i = 0; i < InventorySizeLimit; i++)
        {
            if (InventoryStacks[i] == null && amtLeftToAdd > 0)
            {
                InventoryStacks[i] = new Item_Stack();
                int amtToAdd = Math.Min(InventoryStacks[i].stackLimit, amtLeftToAdd);
                InventoryStacks[i].amount = amtToAdd;
                InventoryStacks[i].resource = type;
                amtLeftToAdd -= amtToAdd;
            }
        }
        updateInventory();
        Debug.Log($"[Invtory] Added {count - amtLeftToAdd} {type.Name}");
        return count - amtLeftToAdd; // Return how many items were actually added
    }

    /// <summary>
    /// Take away resources and update inventory
    /// </summary>
    /// <param name="type"> The type or resource to remove</param>
    /// <param name="count"> Amount to remove </param>
    /// <returns></returns>
    public int RemoveResources(Item_Data type, int count)
    {
        // Error-checking
        if (count < 0)
            Debug.LogError("[Invtry] Cannot remove negative amount"); // not tested
            
        // Track the amount of resources we still need to add
        int amtLeftToRemove = count;

        // Check if there is a slot with the same type and remove if possible
        foreach (Item_Stack istack in InventoryStacks.Reverse())
        {
            // Check if a slot exists with the same type
            if (istack != null && istack.resource == type)
            {
                // Remove as much as we can from this stack
                int amtToRemove = Math.Min(istack.amount, amtLeftToRemove);
                istack.amount -= amtToRemove;
                amtLeftToRemove -= amtToRemove;
            }
        }

        updateInventory(); // Remove empty elements
        Debug.Log($"[Invtory] Removed {count - amtLeftToRemove} {type.Name}");
        // Return however much was added
        return count - amtLeftToRemove;
    }

    /// <summary>
    /// Remove empty items from inventory
    /// </summary>
    private void updateInventory()
    {
        for (int i = 0; i < InventoryStacks.Length; i++)
        {
            if (InventoryStacks[i] != null && InventoryStacks[i].amount <= 0)
                InventoryStacks[i] = null;
            // Display inventory update code here maybe (real not fake, UI stuff)  
        }
        PrintInventory();  
    }

    public int AddDish(Dish_Data dish, int count = 1)
    {
        //     if (inventoryCurrentCount >= InventorySizeLimit)
        //     {
        //         Debug.Log("[Invtry] Inventory full (cannot add dish)");
        //         return 0;
        //     }

        //     int numToAdd = Math.Min(InventorySizeLimit - inventoryCurrentCount, count);

        //     if (dishDict.ContainsKey(dish))
        //     {
        //         dishDict[dish] += numToAdd;
        //     }
        //     else
        //     {
        //         dishDict.Add(dish, numToAdd);
        //     }

        //     inventoryCurrentCount += numToAdd;
        //     Debug.Log($"[Invtry] Added {numToAdd} {dish.Name}");
        // return numToAdd;
        return 1;
    }

    public bool RemoveDish(Dish_Data dish)
    {
        //     if (dishDict.ContainsKey(dish) && dishDict[dish] > 0)
        //     {
        //         dishDict[dish]--;

        //         if (dishDict[dish] == 0)
        //         {
        //             dishDict.Remove(dish);
        //         }

        //         inventoryCurrentCount--;
        //         Debug.Log($"[Invtry] Removed 1 {dish.Name}");
        //         return true;
        //     }

        //     Debug.Log($"[Invtry] Tried to remove {dish.Name}, but none in inventory");
        //     return false;
        return true;
    }

    public bool HasDish(Dish_Data dish)
    {
        //return dishDict.ContainsKey(dish) && dishDict[dish] > 0;
        return true;
    }


    /// <summary>
    /// Print inventory contents
    /// </summary>
    public void PrintInventory() // Not really tested
    {
        foreach (Item_Stack i in InventoryStacks)
        {
            if (i == null)
                Debug.Log($"[Invtry] {i} null");
            else
                Debug.Log($"[Invtry] {i.resource.Name} {i.amount}");           
        }
    }
}
