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
  private bool leftSlotSelected = true;

  // How many stacks this inventory can have
  [field: System.NonSerialized] public override int InventorySizeLimit { get; set; } = 2;

    // Testing
    // public Dish_Data TEST_DISH;

  new private void Awake()
  {
    if (Instance != null && Instance != this)
      Destroy(gameObject);
    else
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }

    // Create inventory stack structure to hold Dish_Tool_Stacks
    InitializeInventoryStacks<Dish_Tool_Stack>();
    updateInventory();

    // AddResources(TEST_DISH, 1);
    // AddResources(TEST_DISH, 3);
  }

  /// <summary>
  /// Overrides the base inventory AddResources function to use the Dish_Tool_Stack type instead.
  /// This is necessary to use the new StackLimit of 1 instead of the default of 20
  /// Also allows for other type differences later on.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="count"></param>
  /// <returns>How many items were added</returns>
  public override int AddResources(Item_Data type, int count)
  {
    return addResourcesOfType<Dish_Tool_Stack>(type, count);
  }

  /// <summary>
  /// Method allows other scripts to set which slot is selected. This function is in Dish_Tool_Inventory.
  /// Used in Key_Listener.
  /// </summary>
  /// <param name="slot"></param>
  public void SetSlotSelected(int slot)
  {
    if (slot == 1)
      leftSlotSelected = true;
    else
      leftSlotSelected = false;
  }

  /// <summary>
  /// Used to remove the dish at the currently selected slot. Pressing key 1 selects
  /// left slot (slot at 0), pressing key 2 selects right slot (slot at 1).
  /// This function is in Dish_Tool_Inventory.
  /// </summary>
  public void RemoveSelectedSlot()
  {
    if (leftSlotSelected)
      InventoryStacks[0] = null;
    else
      InventoryStacks[1] = null;

    updateInventory();
  }

  /// <summary>
  /// Returns Dish_Data for the selected dish. If selectedSlot has no dish, returns null.
  /// This function is in Dish_Tool_Inventory. Used in Customer_Controller.
  /// </summary>
  /// <returns></returns>
  public Dish_Data GetSelectedDishData()
  {
    if (leftSlotSelected)
    {
      if (InventoryStacks[0] == null)
        return null;

      return (Dish_Data)(InventoryStacks[0].resource);
    }

    if (InventoryStacks[1] == null)
      return null;

    return (Dish_Data)(InventoryStacks[1].resource);
  }

  // public bool SelectedSlotHasDish(Dish_Data dish)
  // {
  //   if (leftSlotSelected)
  //   {
  //     if (InventoryStacks[0] == null)
  //       return false;

  //     return (Dish_Data)(InventoryStacks[0].resource) == dish;
  //   }

  //   if (InventoryStacks[1] == null)
  //     return false;

  //   return (Dish_Data)(InventoryStacks[1].resource) == dish;
  // }
}
