using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Close_Inventory_Quest_Step : Dialogue_Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onInventoryToggle += InventoryOpened;
    
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onInventoryToggle -= InventoryOpened;
    }

    void Start()
    {
        if (Tutorial_Manager.Instance.hasClosedInventory)
            FinishQuestStep();
        else
            DelayedDialogue(10, 0, false);
    }


    private void InventoryOpened(bool isOpen)
    {
        if (!isOpen)
            FinishQuestStep(); // Finish and destroy this object
    }
}
