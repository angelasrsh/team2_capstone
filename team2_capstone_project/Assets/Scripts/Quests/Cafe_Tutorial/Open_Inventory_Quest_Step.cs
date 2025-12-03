using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Inventory_Quest_Step : Dialogue_Quest_Step
{
    public List<ItemToAdd> startingIngredients;
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onInventoryToggle += InventoryOpened;
        base.OnEnable();
        
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onInventoryToggle -= InventoryOpened;
        base.OnEnable();
    }

    void Start()
    {
        // Give the player starting ingredients
        foreach (ItemToAdd ing in startingIngredients)
        {
            Ingredient_Inventory.Instance.AddResources(ing.ingredient, ing.amount);    
        }

        dm = FindObjectOfType<Dialogue_Manager>();

        if (dm != null)
        {
            UnityEngine.Debug.Log("[Tutorial] Manually ending current dialogue.");
            dm.EndDialog();
        }
        
        // End if the player already knows the inventory for some reason
        // if (Tutorial_Manager.Instance.hasOpenedInventory)
        //     FinishQuestStep();
        // else
        DelayedDialogue(0, 0, false);
    }

    private void InventoryOpened(bool isOpen)
    {
        if (isOpen)
        {
            QuestStepComplete = true; // Finish and destroy this object
            DelayedDialogue(0, 0, false, postStepTextKey); 
            WaitFinishQuestStep(7);        
        }
            
        if (dialogueComplete)
            WaitFinishQuestStep(7);
    }
}


[System.Serializable]
public class ItemToAdd
{
    public Ingredient_Data ingredient;
    public int amount;
}
