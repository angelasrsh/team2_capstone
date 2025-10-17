using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Inventory_Quest_Step : Dialogue_Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onInventoryToggle += InventoryOpened;
        base.OnEnable();
        
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onInventoryToggle -= InventoryOpened;
        base.OnEnable();
    }

    void Start()
    {
        // End if the player already knows the inventory for some reason
        if (Tutorial_Manager.Instance.hasOpenedInventory)
            FinishQuestStep();
        else
            DelayedDialogue(0, 0, false);
    }


    private void InventoryOpened(bool isOpen)
    {
        if (isOpen)
        {
            QuestStepComplete = true; // Finish and destroy this object
            DelayedDialogue(0, 0, false, postStepTextKey);         
        }
            

        if (dialogueComplete)
            FinishQuestStep();
    }
}
