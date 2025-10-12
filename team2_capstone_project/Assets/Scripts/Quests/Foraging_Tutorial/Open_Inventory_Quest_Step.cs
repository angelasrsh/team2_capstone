using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Inventory_Quest_Step : Quest_Step
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


    private void InventoryOpened(bool isOpen)
    {
        if (isOpen)
            FinishQuestStep(); // Finish and destroy this object
    }
}
