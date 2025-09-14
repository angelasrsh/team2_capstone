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
    //private int inventoryCurrentCount = 0;

    //[SerializeField] private List<Item_Stack> resourceListInspector = new List<Resource_Stack>();
    //[SerializeField] private List<Dish_Stack> dishListInspector = new List<Dish_Stack>();

    // Runtime dictionaries for efficient lookups
    // private Dictionary<Item_Data, int> resourceDict = new Dictionary<Item_Data, int>();
    // private Dictionary<Dish_Data, int> dishDict = new Dictionary<Dish_Data, int>();

    [field: SerializeField]
    private Item_Stack[] InventoryStacks;
    public Item_Data TestItem;
    public Item_Data TestItem2;


    private void Awake()
    {
        InventoryStacks = new Item_Stack[InventorySizeLimit];
        Item_Data milk = TestItem;
        Item_Data cheese = TestItem2;
        Debug.Log($"Milk {milk} .");
        Debug.Log($"Add {AddResources(milk, 30)} milk");
        Debug.Log($"Removed {RemoveResources(cheese, 10)} cheese");
        Debug.Log($"Removed {RemoveResources(milk, 40)} milk");
        Debug.Log($"Add {AddResources(cheese, 230)} cheese");
        Debug.Log($"Add {AddResources(cheese, 50)} cheese");
        Debug.Log($"Removed {RemoveResources(cheese, 100)} cheese");
        Debug.Log($"Removed {RemoveResources(cheese, 10)} cheese");


        // // Convert inspector lists into runtime dictionaries
        // foreach (var stack in resourceListInspector)
        // {
        //     if (stack.resource != null && stack.amount > 0)
        //     {
        //         resourceDict[stack.resource] = stack.amount;
        //         inventoryCurrentCount += stack.amount;
        //     }
        // }

        // foreach (var stack in dishListInspector)
        // {
        //     if (stack.dish != null && stack.amount > 0)
        //     {
        //         dishDict[stack.dish] = stack.amount;
        //         inventoryCurrentCount += stack.amount;
        //     }
        // }
    }

    public int AddResources(Item_Data type, int count)
    {
        // Track the amount of resources we still need to add
        int amtLeftToAdd = count;

        // Check if there is a slot with the same type and add if not full
        foreach (Item_Stack istack in InventoryStacks)
        {
            // Check if a slot exists with the same type
            if (istack != null && istack.resource == type)
            {
                if (istack.amount < istack.stackLimit)
                {
                    // Add as much as we can to this stack
                    int amtToAdd = Math.Min(istack.stackLimit - istack.amount, amtLeftToAdd);
                    istack.amount += amtToAdd;
                    amtLeftToAdd -= amtToAdd;
                }
            }
        }

        // We were not able to add all items to existing slots, so check if we can start a new stack
        // These are two separate loops because we don't assume slots will be filled in order
        for (int i = 0; i < InventorySizeLimit; i++) {
            if (InventoryStacks[i] == null)
            {
                InventoryStacks[i] = new Item_Stack();
                int amtToAdd = Math.Min(InventoryStacks[i].stackLimit, amtLeftToAdd);
                InventoryStacks[i].amount = amtToAdd;
                InventoryStacks[i].resource = type;
                amtLeftToAdd -= amtToAdd;
            }
        }

        updateInventory();
        return count - amtLeftToAdd; // Return how many items were actually added
    }



    // // Create a new slot if there is one

    //         // Return 0 and fail if not
    //         if (inventoryCurrentCount >= InventorySizeLimit)
    //         {
    //             Debug.Log("[Invtry] Inventory full");
    //             return 0;
    //         }

    // int numToAdd = Math.Min(InventorySizeLimit - inventoryCurrentCount, count);
    // if (resourceDict.ContainsKey(type))
    // {
    //     resourceDict[type] += numToAdd;
    // }
    // else
    // {
    //     resourceDict.Add(type, numToAdd);
    // }

    // inventoryCurrentCount += numToAdd;
    // Debug.Log("[Invtry] Added " + numToAdd + " " + type.name);
    // return numToAdd;


    // Remove count number of a specific resource or as much of it that exists
    // Return the number removed
    // public int RemoveResources(Item_Data type, int count)
    // {
    //     int numToRemove = 0;
    //     if (resourceDict.ContainsKey(type))
    //     {
    //         numToRemove = Math.Min(resourceDict[type], count);
    //         resourceDict[type] -= numToRemove;

    //         // Clean up list if none of an item exists
    //         if (resourceDict[type] == 0)
    //         {
    //             resourceDict.Remove(type);
    //         }
    //     }

    //     inventoryCurrentCount -= numToRemove;
    //     Debug.Log($"[Invtry] Removed {numToRemove} {type.Name}");
    //     return numToRemove;
    // }

    /// <summary>
    /// Take away resources and update inventory
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count">Myst be non-negative</param>
    /// <returns></returns>
    public int RemoveResources(Item_Data type, int count)
    {
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
        // Return however much was added
        return count - amtLeftToRemove;

        // We were not able to add all items to existing slots, so check if we can start a new stack
        // These are two separate loops because we don't assume slots will be filled in order
        // for (int i = 0; i < InventorySizeLimit; i++) {
        //     if (InventoryStacks[i] == null)
        //     {
        //         InventoryStacks[i] = new Item_Stack { resource = type };
        //         int amtToAdd = Math.Min(InventoryStacks[i].stackLimit, amtLeftToAdd);
        //         InventoryStacks[i].amount = amtToAdd;
        //         amtLeftToAdd -= amtToAdd;
        //     }

        //     // Finish if we've added everything
        //     if (amtLeftToAdd == 0)
        //         return count;
        // }

        // return amtLeftToAdd; // Unable to add everything
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

            DisplayInventory();
            
        }
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


    // TODO: Need to recreate the displayInventory for player input code, maybe as input event
    public void DisplayInventory()
    {
        Debug.Log(InventoryStacks);
        // if (resourceDict.Count == 0 && dishDict.Count == 0)
        // {
        //     Debug.Log("[Invtry] Inventory is empty");
        //     return;
        // }

        // Debug.Log($"[Invtry] Limit: {InventorySizeLimit} Count: {inventoryCurrentCount}");

        // foreach (KeyValuePair<Item_Data, int> kvp in resourceDict)
        // {
        //     Debug.Log($"[Invtry] Resource = {kvp}, Amount = {kvp.Value}");
        // }

        // foreach (KeyValuePair<Dish_Data, int> kvp in dishDict)
        // {
        //     Debug.Log($"[Invtry] Dish = {kvp}, Amount = {kvp.Value}");
        // }
    }
}
