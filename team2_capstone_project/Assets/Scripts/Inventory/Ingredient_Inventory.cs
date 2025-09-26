using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Derived class of Inventory specifically for ingredients.
/// Uses the singleton pattern. Call functions using Ingredient_Inventory.Instance.<...>
/// 
/// Also contains ingredient specific methods, including add/remove resources for the IngredientType enum
/// and converting an Ingredient_Data to the enum.
/// This requires an updated list of all ingredients that need to be converted! This is annoying
/// </summary>
public class Ingredient_Inventory : Inventory
{
  public static Ingredient_Inventory Instance { get; private set; }

  // Must drop all ingredient lists into here! Sorry -V('.')V-
  public List<Ingredient_Data> AllIngredientList;
  private Ingredient_Data water;
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
      if (idata.Name == "Water")
        water = idata;
    }
  }

    /// <summary>
    /// Overload AddResources to allow for using the IngredientType enum
    /// </summary>
    /// <param name="type"> IngredientType enum for ingredients </param>
    /// <param name="count"> Amount to add </param>
    /// <returns> Amount actually added </returns>
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

         // Broadcast to listening events
        Game_Events_Manager.Instance.ResourceAdd(IngrEnumToData(type));

        updateInventory();
        Debug.Log($"[Invtory] Added {count - amtLeftToAdd} {IngrEnumToData(type).Name}");
        return count - amtLeftToAdd; // Return how many items were actually added
    }

  /// <summary>
  /// Overload base inventory RemoveResources function to allow removing ingredients using type enum
  /// </summary>
  /// <param name="type"> type to remove </param>
  /// <param name="count"> amount to remove </param>
  /// <returns> Amount of type actually removed </returns>
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

    // Broadcast to listening events
    Game_Events_Manager.Instance.ResourceRemove(IngrEnumToData(type));

    updateInventory(); // Remove empty elements
    Debug.Log($"[Invtory] Removed {count - amtLeftToRemove} {IngrEnumToData(type).Name}");
    // Return however much was added
    return count - amtLeftToRemove;
  }

  /// <summary>
  /// Convert enum IngredientType to a name string.
  /// String here must match Item_Data Name!
  /// </summary>
  /// <param name="t"> type to convert to a string </param>
  /// <returns></returns>
  public String IngrEnumToName(IngredientType t)
  {
    switch (t)
    {
      case IngredientType.Egg:
        return "Egg";
      case IngredientType.Milk:
        return "Milk";
      case IngredientType.Cheese:
        return "Cheese";
      case IngredientType.Melon:
        return "Melon";
      case IngredientType.Uncut_Fogshroom:
        return "Uncut Fogshroom";
      case IngredientType.Uncut_Fermented_Eye:
        return "Uncut Fermented Eye";
      case IngredientType.Slime:
        return "Slime";
      case IngredientType.Bone_Broth:
        return "Bone Broth";
      case IngredientType.Bone:
        return "Bone";
      case IngredientType.Cut_Fermented_Eye:
        return "Cut Fermented Eye";
      case IngredientType.Cut_Fogshroom:
        return "Cut Fogshroom";
      default:
        return "";
    }
  }

  /// <summary>
  /// For use by Inventory_Overlap
  /// Converts Ingredient_Data to IngredientType
  /// </summary>
  /// <param name="d"> Ingredient_Data to convert to an enum </param>
  /// <returns> The corresponding IngredientType enum </returns>
  public IngredientType IngrDataToEnum(Ingredient_Data d)
  {
    if (d == null)
      return IngredientType.Null;

    switch (d.Name)
    {
      case "Egg":
        return IngredientType.Egg;
      case "Milk":
        return IngredientType.Milk;
      case "Cheese":
        return IngredientType.Cheese;
      case "Melon":
        return IngredientType.Melon;
      case "Uncut Fogshroom":
        return IngredientType.Uncut_Fogshroom;
      case "Uncut Fermented Eye":
        return IngredientType.Uncut_Fermented_Eye;
      case "Slime":
        return IngredientType.Slime;
      case "Bone Broth":
        return IngredientType.Bone_Broth;
      case "Bone":
        return IngredientType.Bone;
      case "Cut Fermented Eye":
        return IngredientType.Cut_Fermented_Eye;
      case "Cut Fogshroom":
        return IngredientType.Cut_Fogshroom;
      default:
        return IngredientType.Null;
    }

  }

  /// <summary>
  /// Get the corresponding Ingredient_Data scriptable object for a given IngredientType enum.
  /// 
  /// Requires that the Ingredient_Inventory AllIngredientsList is up to date!
  /// 
  /// Must be better ways to do this but for now, this is the best I've got.
  /// </summary>
  /// <param name="t"> Ingredient enum </param>
  /// <returns> Reference to a corresponding Ingredient_Data scriptable object </returns>
  public Ingredient_Data IngrEnumToData(IngredientType t)
  {
    String name = IngrEnumToName(t);
    if (IngredientDict.ContainsKey(name))
      return IngredientDict[IngrEnumToName(t)];
    else
      Debug.LogError($"[Ingr_Intry] {IngrEnumToName(t)} not found in ingredient dictionary");
    return null;
  }

  /// <summary>
  /// get the total count of a specific ingredient in the inventory by name
  /// used for checking if we have enough ingredients to make a dish in Journal_Menu
  /// </summary>
  /// <param name="ingredientName"></param>
  /// <returns></returns>
  public int GetItemCount(string ingredientName)
  {
    int total = 0;
    foreach (Item_Stack stack in InventoryStacks)
    {
      if (stack != null && stack.resource != null && stack.resource.Name == ingredientName)
      {
        total += stack.amount;
      }
    }
    return total;
  }

  /// <returns>Water's ingredient data</returns>
  public Ingredient_Data getWaterData()
  {
    return water;
  }
}
