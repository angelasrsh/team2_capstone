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

  protected override void Awake()
  {
    // Singleton enforcement
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    // Initialize Dish_Tool inventory specifically
    InventorySizeLimit = 2;
    InitializeInventoryStacks<Dish_Tool_Stack>();

    // AddResources(TEST_DISH, 1);
    // AddResources(TEST_DISH, 3);
  }
  
  private void Start()
  {
    // Delay to Start to ensure Inventory_Grid is also initialized
    updateInventory();
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
    {
      leftSlotSelected = true;
      // Debug.Log("Selected dish inventory slot 1");
    }
    else
    {
      leftSlotSelected = false;
      // Debug.Log("Selected dish inventory slot 2");
    }
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

    /// <summary>
    /// Returns true if the inventory is completely full
    /// </summary>
    /// <returns> true if # items = total possible number of items </returns>
    public bool IsFull()
    {
        bool isFull = (TotalIngCount == InventorySizeLimit);
        return isFull;
        
    }
  
      #region Save / Load
    public DishInventoryData GetSaveData()
    {
        DishInventoryData data = new DishInventoryData();

        data.InventoryStacks = this.InventoryStacks;
        data.TotalIngCount = this.TotalIngCount;

        return data;
    }

    public void LoadFromSaveData(DishInventoryData data)
    {
        if (data == null)
        {
            Helpers.printLabeled(this, "No dish data to load; initializing defaults.");
            return;
        }

        this.InventoryStacks = data.InventoryStacks;
        this.TotalIngCount = data.TotalIngCount;

        Debug.Log("Dish Inventory data loaded successfully.");
    }
    #endregion
}

#region Dish_Inventory_Save_Data
[System.Serializable]
public class DishInventoryData
{
    [field: SerializeField] public Item_Stack[] InventoryStacks;

    // Count total amount of items
    public int TotalIngCount = 0;
}
#endregion

