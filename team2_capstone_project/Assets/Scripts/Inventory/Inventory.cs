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

[System.Serializable]
public class Resource_Stack
{
    public Item_Data resource;
    public int amount;
}

[System.Serializable]
public class Dish_Stack
{
    public Dish_Data dish;
    public int amount;
}


// Gives the player a collection of items of a fixed size
public class Inventory : MonoBehaviour
{
    public int InventorySizeLimit = 12;
    private int inventoryCurrentCount = 0;

    [SerializeField] private List<Resource_Stack> resourceListInspector = new List<Resource_Stack>();
    [SerializeField] private List<Dish_Stack> dishListInspector = new List<Dish_Stack>();

    // Runtime dictionaries for efficient lookups
    private Dictionary<Item_Data, int> resourceDict = new Dictionary<Item_Data, int>();
    private Dictionary<Dish_Data, int> dishDict = new Dictionary<Dish_Data, int>();

    private void Awake()
    {
        // Convert inspector lists into runtime dictionaries
        foreach (var stack in resourceListInspector)
        {
            if (stack.resource != null && stack.amount > 0)
            {
                resourceDict[stack.resource] = stack.amount;
                inventoryCurrentCount += stack.amount;
            }
        }

        foreach (var stack in dishListInspector)
        {
            if (stack.dish != null && stack.amount > 0)
            {
                dishDict[stack.dish] = stack.amount;
                inventoryCurrentCount += stack.amount;
            }
        }
    }

    public int AddResources(Item_Data type, int count)
    {
        if (inventoryCurrentCount >= InventorySizeLimit)
        {
            Debug.Log("[Invtry] Inventory full");
            return 0;
        }

        int numToAdd = Math.Min(InventorySizeLimit - inventoryCurrentCount, count);
        if (resourceDict.ContainsKey(type))
        {
            resourceDict[type] += numToAdd;
        }
        else
        {
            resourceDict.Add(type, numToAdd);
        }

        inventoryCurrentCount += numToAdd;
        Debug.Log("[Invtry] Added " + numToAdd + " " + type.name);
        return numToAdd;
    }

    // Remove count number of a specific resource or as much of it that exists
    // Return the number removed
    public int RemoveResources(Item_Data type, int count)
    {
        int numToRemove = 0;
        if (resourceDict.ContainsKey(type))
        {
            numToRemove = Math.Min(resourceDict[type], count);
            resourceDict[type] -= numToRemove;

            // Clean up list if none of an item exists
            if (resourceDict[type] == 0)
            {
                resourceDict.Remove(type);
            }
        }

        inventoryCurrentCount -= numToRemove;
        Debug.Log($"[Invtry] Removed {numToRemove} {type.Name}");
        return numToRemove;
    }

    public int AddDish(Dish_Data dish, int count = 1)
    {
        if (inventoryCurrentCount >= InventorySizeLimit)
        {
            Debug.Log("[Invtry] Inventory full (cannot add dish)");
            return 0;
        }

        int numToAdd = Math.Min(InventorySizeLimit - inventoryCurrentCount, count);

        if (dishDict.ContainsKey(dish))
        {
            dishDict[dish] += numToAdd;
        }
        else
        {
            dishDict.Add(dish, numToAdd);
        }

        inventoryCurrentCount += numToAdd;
        Debug.Log($"[Invtry] Added {numToAdd} {dish.Name}");
        return numToAdd;
    }

    public bool RemoveDish(Dish_Data dish)
    {
        if (dishDict.ContainsKey(dish) && dishDict[dish] > 0)
        {
            dishDict[dish]--;

            if (dishDict[dish] == 0)
            {
                dishDict.Remove(dish);
            }

            inventoryCurrentCount--;
            Debug.Log($"[Invtry] Removed 1 {dish.Name}");
            return true;
        }

        Debug.Log($"[Invtry] Tried to remove {dish.Name}, but none in inventory");
        return false;
    }

    public bool HasDish(Dish_Data dish)
    {
        return dishDict.ContainsKey(dish) && dishDict[dish] > 0;
    }


    public void DisplayInventory(InputAction.CallbackContext context)
    {
        if (resourceDict.Count == 0 && dishDict.Count == 0)
        {
            Debug.Log("[Invtry] Inventory is empty");
            return;
        }

        Debug.Log($"[Invtry] Limit: {InventorySizeLimit} Count: {inventoryCurrentCount}");

        foreach (KeyValuePair<Item_Data, int> kvp in resourceDict)
        {
            Debug.Log($"[Invtry] Resource = {kvp}, Amount = {kvp.Value}");
        }

        foreach (KeyValuePair<Dish_Data, int> kvp in dishDict)
        {
            Debug.Log($"[Invtry] Dish = {kvp}, Amount = {kvp.Value}");
        }
    }
}
