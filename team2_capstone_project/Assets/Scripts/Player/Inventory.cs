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

// Gives the player a collection of items of a fixed size
public class Inventory : MonoBehaviour
{
    // Inventory specifications
    public int InventorySizeLimit = 10;

    // Inventory status
    private int inventoryCurrentCount = 0;


    [field: SerializeField] private Dictionary<ResourceInfo, int> ResourceList { get; set; } = new Dictionary<ResourceInfo, int>();
    [field: SerializeField] private Dictionary<DishData, int> DishList { get; set; } = new Dictionary<DishData, int>();


    // Add resources 
    // Return the number added

    public int AddResources(ResourceInfo type, int count)
    {
        if (inventoryCurrentCount >= InventorySizeLimit)
        {
            Debug.Log("[Invtry] Inventory full");
            return 0;
        }

        int numToAdd = Math.Min(InventorySizeLimit - inventoryCurrentCount, count);
        if (ResourceList.ContainsKey(type))
        {
            ResourceList[type] += numToAdd;
        }
        else
        {
            ResourceList.Add(type, numToAdd);
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
        if (ResourceList.ContainsKey(type))
        {
            numToRemove = Math.Min(ResourceList[type], count);
            ResourceList[type] -= numToRemove;

            // Clean up list if none of an item exists
            if (ResourceList[type] == 0)
            {
                ResourceList.Remove(type);
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

        if (DishList.ContainsKey(dish))
        {
            DishList[dish] += numToAdd;
        }
        else
        {
            DishList.Add(dish, numToAdd);
        }

        inventoryCurrentCount += numToAdd;
        Debug.Log($"[Invtry] Added {numToAdd} {dish.dishName}");
        return numToAdd;
    }

    public bool RemoveDish(DishData dish)
    {
        if (DishList.ContainsKey(dish) && DishList[dish] > 0)
        {
            DishList[dish]--;

            if (DishList[dish] == 0)
            {
                DishList.Remove(dish);
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
        return DishList.ContainsKey(dish) && DishList[dish] > 0;
    }


    public void DisplayInventory(InputAction.CallbackContext context)
    {
        if (ResourceList.Count == 0 && DishList.Count == 0)
        {
            Debug.Log("[Invtry] Inventory is empty");
            return;
        }

        Debug.Log($"[Invtry] Limit: {InventorySizeLimit} Count: {inventoryCurrentCount}");

        foreach (KeyValuePair<ResourceInfo, int> kvp in ResourceList)
        {
            Debug.Log($"[Invtry] Resource = {kvp.Key.Name}, Amount = {kvp.Value}");
        }

        foreach (KeyValuePair<DishData, int> kvp in DishList)
        {
            Debug.Log($"[Invtry] Dish = {kvp.Key.dishName}, Amount = {kvp.Value}");
        }
    }
}
