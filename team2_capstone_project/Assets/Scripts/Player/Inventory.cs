using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Android.Types;
using Unity.VisualScripting;
using UnityEditor.iOS;
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

    public void DisplayInventory(InputAction.CallbackContext context)
    {
        if (ResourceList.Count == 0)
        {
            Debug.Log("[Invtry] Inventory is empty");
        }

        Debug.Log($"[Invtry] Limit: {InventorySizeLimit} Count: {inventoryCurrentCount}");
        foreach (KeyValuePair<ResourceInfo, int> kvp in ResourceList)
        {
            Debug.Log($"[Invtry] Item = {kvp.Key.Name}, Value = {kvp.Value}");
        }


    }

}
