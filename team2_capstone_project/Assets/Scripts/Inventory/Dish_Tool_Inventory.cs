using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// You can currently store dishes indefinitely. Probably not desired behavior,
/// but I'm leaving it for now. We probably change it semi-easily later.
/// 
/// </summary>
public class Dish_Tool_Inventory : Inventory
{
  public static Dish_Tool_Inventory Instance { get; protected set; }
  private bool leftSlotSelected = true;
  // private static int dishesPerSlot = 1;

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
    ItemStackLimit = 1;
    InitializeInventoryStacks();

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
      if (InventoryStacks == null || InventoryStacks.Length != InventorySizeLimit)
      {
          Debug.LogWarning("[Dish_Tool_Inventory] InventoryStacks not initialized properly — rebuilding.");
          InitializeInventoryStacks();
      }

      Debug.Log($"[Dish_Tool_Inventory]: Adding {count} of {type.name} to dish inventory.");
      return addResourcesOfType(type, count);
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
        // Ensure array is initialized and valid
        if (InventoryStacks == null || InventoryStacks.Length < 2)
        {
            Debug.LogWarning("[Dish_Tool_Inventory] InventoryStacks was null or wrong size — rebuilding.");
            InventorySizeLimit = 2;
            InitializeInventoryStacks();
        }

        if (leftSlotSelected)
        {
            if (InventoryStacks[0].resource == null)
            {
                Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
                dm.PlayScene("Default.LeftHandEmpty");
                return null;
            }
                

            return (Dish_Data)InventoryStacks[0].resource;
        }

        if (InventoryStacks[1].resource == null)
        {
            Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
            dm.PlayScene("Default.RightHandEmpty");
            return null;
        }

        return (Dish_Data)InventoryStacks[1].resource;
    }
    
  
      #region Save / Load
    public DishInventoryData GetSaveData()
    {
        DishInventoryData data = new DishInventoryData();

        data.InventoryStacks = this.InventoryStacks;

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

        Debug.Log("Dish Inventory data loaded successfully.");
    }
    #endregion
}

#region Dish_Inventory_Save_Data
[System.Serializable]
public class DishInventoryData
{
    [field: SerializeField] public Item_Stack[] InventoryStacks;

}
#endregion

