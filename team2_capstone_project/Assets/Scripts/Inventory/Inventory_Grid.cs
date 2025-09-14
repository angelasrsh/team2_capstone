using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Inventory_Type {
    Dish,
    Ingredient
}

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
        // Set inventory
        switch (InventoryType)
        {
            case Inventory_Type.Dish:
                inventory = GameObject.Find("Ingredient_Inventory").GetComponent<Ingredient_Inventory>();
                break;
            case Inventory_Type.Ingredient:
                inventory = GameObject.Find("Dish_Inventory").GetComponent<Dish_Inventory>();
                break;
            default:
                break;
        }

        if (inventory == null)
            Debug.LogError("[Invtry_Grd] No inventory to populate inventory grid!");
    }

    /// <summary>
    /// Fill inventory grid from inventory
    /// </summary>
    private void PopulateInventory()
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
