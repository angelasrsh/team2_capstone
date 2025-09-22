using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// For setting the type of the grid in the inspector. May not be necessary since there are only two inventory types.
/// </summary>
public enum Inventory_Type
{
    Dish,
    Ingredient
}

/// <summary>
/// For populating an Inventory UI. 
/// To use this class, add an Inventory_Grid prefab into a scene and set its inventory type (dish or ingredient)
/// Make sure the Dish_Inventory and/or Ingredient_Inventory manager exists in the scene.
/// 
/// On Start, the grid will find and connect itself to an inventory instance.
/// Adding or removing resources from the instance will call PopulateInventory to refresh its visuals in the grid UI.
/// </summary>
public class Inventory_Grid : MonoBehaviour
{
    // Populate all cells in the grid

    // Choose which inventory to display (dish or ingredient)
    public Inventory_Type InventoryType;
    [field: SerializeField] private Inventory inventory;
    void Start()
    {
        SetInventory();
        PopulateInventory();
    }

    /// <summary>
    /// Find the inventory instance we want. Dish inventory not tested.
    /// </summary>
    private void SetInventory()
    {
        // Based on InventoryType, find the right manager and attach to it
        switch (InventoryType)
        {
            case Inventory_Type.Dish:
                inventory = Dish_Tool_Inventory.Instance;
                Dish_Tool_Inventory.Instance.InventoryGrid = this;
                break;
            case Inventory_Type.Ingredient:
                inventory = Ingredient_Inventory.Instance;
                Ingredient_Inventory.Instance.InventoryGrid = this;
                break;
            default:
                break;
        }

        if (inventory == null)
            Debug.LogError("[Invtry_Grd] No inventory to populate inventory grid!");
    }

    /// <summary>
    /// Fill inventory grid from connected inventory data
    /// </summary>
    public void PopulateInventory()
    {
        // Variables to count how many UIslots/stacks we've seen/used
        int countGridSlots = 0;
        int inventorySlotIndex = 0;
        // Populate all child inventory slots from the
        foreach (Transform childTransform in transform)
        {
            Inventory_Slot slot = childTransform.gameObject.GetComponent<Inventory_Slot>();

            if (slot == null) // not a valid slot
                continue;

            countGridSlots++;

            if (inventorySlotIndex < inventory.InventoryStacks.Length)
            {
                slot.PopulateSlot(inventory.InventoryStacks[inventorySlotIndex]);
                inventorySlotIndex++;
            }
            else // If more grid slots than inventory, clear out slots
                slot.PopulateSlot(null);


        }

        if (countGridSlots != inventory.InventorySizeLimit)
            Debug.LogWarning("[Intry_Grd] number of UI slots and inventory size do not match");
    }

}
