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
public class ResourceStack
{
    public ResourceInfo resource;
    public int amount;
}

[System.Serializable]
public class DishStack
{
    public DishData dish;
    public int amount;
}


// Gives the player a collection of items of a fixed size
public class Inventory : MonoBehaviour
{
    public int InventorySizeLimit = 12;
    private int inventoryCurrentCount = 0;

    [SerializeField] private List<ResourceStack> resourceListInspector = new List<ResourceStack>();
    [SerializeField] private List<DishStack> dishListInspector = new List<DishStack>();

    // Runtime dictionaries for efficient lookups
    private Dictionary<ResourceInfo, int> resourceDict = new Dictionary<ResourceInfo, int>();
    private Dictionary<DishData, int> dishDict = new Dictionary<DishData, int>();

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

    public int AddResources(ResourceInfo type, int count)
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
    public int RemoveResources(ResourceInfo type, int count)
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

    public int AddDish(DishData dish, int count = 1)
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
        Debug.Log($"[Invtry] Added {numToAdd} {dish.dishName}");
        return numToAdd;
    }

    public bool RemoveDish(DishData dish)
    {
        if (dishDict.ContainsKey(dish) && dishDict[dish] > 0)
        {
            dishDict[dish]--;

            if (dishDict[dish] == 0)
            {
                dishDict.Remove(dish);
            }

            inventoryCurrentCount--;
            Debug.Log($"[Invtry] Removed 1 {dish.dishName}");
            return true;
        }

        Debug.Log($"[Invtry] Tried to remove {dish.dishName}, but none in inventory");
        return false;
    }

    public bool HasDish(DishData dish)
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

        foreach (KeyValuePair<ResourceInfo, int> kvp in resourceDict)
        {
            Debug.Log($"[Invtry] Resource = {kvp.Key.Name}, Amount = {kvp.Value}");
        }

        foreach (KeyValuePair<DishData, int> kvp in dishDict)
        {
            Debug.Log($"[Invtry] Dish = {kvp.Key.dishName}, Amount = {kvp.Value}");
        }
    }
}
