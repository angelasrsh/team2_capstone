using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Grid : MonoBehaviour
{
    // Populate all cells in the grid

    // Choose which inventory to display (dish or ingredient)
    public Inventory inventory;
    void Start()
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
