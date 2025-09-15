using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ingredient_Inventory : Inventory
{
    public static Ingredient_Inventory Instance { get; private set; }

    // Must drop all ingredient lists into here! Sorry -V('.')V-
    public List<Ingredient_Data> AllIngredientList;
    public Dictionary<String, Ingredient_Data> IngredientDict = new Dictionary<string, Ingredient_Data>(); // Make private once we know it works

    new private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        base.Awake();

        // Put the list of ingredients into the dictionary to be accessed by their name string
        foreach (Ingredient_Data idata in AllIngredientList)
        {
            IngredientDict.Add(idata.Name, idata);
        }

        AddResources(IngredientType.Egg, 10);
        AddResources(IngredientType.Cheese, 10);
        AddResources(IngredientType.Morel, 10);
        AddResources(IngredientType.Milk, 10);
        RemoveResources(IngredientType.Milk, 5);
    }

    /// <summary>
    /// Overload AddResources to allow for using the IngredientType enum
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int AddResources(IngredientType type, int count)
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
            if (istack != null && istack.resource == IngrEnumToData(type) && istack.amount < istack.stackLimit)
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
                InventoryStacks[i].resource = IngrEnumToData(type);
                amtLeftToAdd -= amtToAdd;
            }
        }
        updateInventory();
        Debug.Log($"[Invtory] Added {count - amtLeftToAdd} {IngrEnumToData(type).Name}");
        return count - amtLeftToAdd; // Return how many items were actually added

    }

    /// <summary>
    /// Overload base inventory RemoveResources function to allow removing ingredients using type enum
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int RemoveResources(IngredientType type, int count)
    {
        // Error-checking
        if (count < 0)
            Debug.LogError("[Invtry] Cannot remove negative amount"); // not tested

        // Track the amount of resources we still need to add
        int amtLeftToRemove = count;

        // Check if there is a slot with the same type and remove if possible
        foreach (Item_Stack istack in Enumerable.Reverse<Item_Stack>(InventoryStacks)) // Not sure if this works
        {
            // Check if a slot exists with the same type
            if (istack != null && istack.resource == IngrEnumToData(type))
            {
                // Remove as much as we can from this stack
                int amtToRemove = Math.Min(istack.amount, amtLeftToRemove);
                istack.amount -= amtToRemove;
                amtLeftToRemove -= amtToRemove;
            }
        }

        updateInventory(); // Remove empty elements
        Debug.Log($"[Invtory] Removed {count - amtLeftToRemove} {IngrEnumToData(type).Name}");
        // Return however much was added
        return count - amtLeftToRemove;
    }

    /// <summary>
    /// Convert enum IngredientType to a name string.
    /// String here must match Item_Data Name!
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private String IngrEnumToName(IngredientType t)
    {
        switch (t)
        {
            case IngredientType.Egg:
                return "Egg";
            case IngredientType.Morel:
                return "Morel";
            case IngredientType.Milk:
                return "Milk";
            case IngredientType.Cheese:
                return "Cheese";
            case IngredientType.Melon:
                return "Melon";
            default:
                return "";
        }
    }

/// <summary>
/// For use by Inventory_Overlap
/// Converts Ingredient_Data to IngredientType
/// </summary>
/// <param name="d"></param>
/// <returns></returns>
    public IngredientType IngrDataToEnum(Ingredient_Data d)
    {
        if (d == null)
            return IngredientType.Null;
            
        switch (d.Name)
        {
            case "Egg":
                return IngredientType.Egg;
            case "Morel":
                return IngredientType.Morel;
            case "Milk":
                return IngredientType.Milk;
            case "Cheese":
                return IngredientType.Cheese;
            case "Melon":
                return IngredientType.Melon;
            default:
                return IngredientType.Melon;
        }

    }

    /// <summary>
    /// Must be better ways to do this but for now, this is the best I've got.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private Ingredient_Data IngrEnumToData(IngredientType t)
    {
        String name = IngrEnumToName(t);
        if (IngredientDict.ContainsKey(name))
            return IngredientDict[IngrEnumToName(t)];
        else
            Debug.LogError($"[Ingr_Intry] {IngrEnumToName(t)} not found in ingredient dictionary");
        return null;
    }



}
