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



/// <summary>
/// One inventory slot = 1 item stack. Item_Data can be filled with Dish_Data or Ingredient_Data
/// </summary>
[System.Serializable]
public class Item_Stack
{
    public Item_Data resource;
    public int amount;
    public int stackLimit = 20;
}

/// <summary>
/// Inventory base class for the dish_inventory and ingredient_inventory classes.
/// It provides functions general functions for Item_Data types that are used by dish and ingredient inventories)
/// This inventory is not called directly. Instead, call functions using the instances of the child inventories.
/// 
/// To add inventory items, call AddResources and RemoveResources on an inventory instance.
/// </summary>

// Gives the player a collection of items of a fixed size
public class Inventory : MonoBehaviour
{
    public int InventorySizeLimit = 12;

    [field: SerializeField]
    public Item_Stack[] InventoryStacks { get; private set; }

    /// <summary>
    /// An inventoryGrid will add itself to an Ingredient or Dish inventory on Start() to display its contents.
    /// </summary>
    public Inventory_Grid InventoryGrid;

    /// <summary>
    /// Initialize Inventory to be size InventorySizeLimit
    /// </summary>
    protected void Awake()
    {
        if (InventoryStacks == null)
            InventoryStacks = new Item_Stack[InventorySizeLimit];
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
            if ((InventoryStacks[i] == null || InventoryStacks[i].resource == null) && amtLeftToAdd > 0)
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
    /// <returns> The amount of resources actually removed </returns>
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
    /// Remove empty items from inventory and populate the InventoryGrid display (if one is connected).
    /// </summary>
    protected void updateInventory()
    {
        for (int i = 0; i < InventoryStacks.Length; i++)
        {
            if (InventoryStacks[i] != null && InventoryStacks[i].amount <= 0)
                InventoryStacks[i] = null;
            // Display inventory update code here maybe (real not fake, UI stuff)  
        }
        //PrintInventory();  
        if (InventoryGrid != null)
            InventoryGrid.PopulateInventory();
    }

    /// <summary>
    /// Check if the inventory contains the requested item
    /// </summary>
    /// <param name="item"> Item to search for </param>
    /// <returns> True if the item is in inventory, false otherwise. </returns>
    public bool HasItem(Item_Data item)
    {
        foreach (Item_Stack i in InventoryStacks)
        {
            if (i != null && i.resource == item)
                return true;
        }
        return false;
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
