using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvest_Quest_Step : Quest_Step
{
    [Header("Items to collect")]
    public Ingredient_Data[] RequiredIngredients;
    public int[] ItemCounts;




    void OnEnable()
    {
        Game_Events_Manager.Instance.onHarvest += Harvest;

        if (ItemCounts.Length != RequiredIngredients.Length)
            Debug.Log($"{GetType().Name} on {gameObject.name} Error: Please ensure there is a required count for every item");
       
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onHarvest -= Harvest;
    }


    private void Harvest()
    {
        // Check if all required items have been collected
        bool hasAllItems = true;
        for (int i = 0; i < RequiredIngredients.Length; i++)
        {
            if (Ingredient_Inventory.Instance.GetItemCount(RequiredIngredients[i]) < ItemCounts[i])
            {
                hasAllItems = false;
                break;
            }
                
        }

        if (hasAllItems)
        {
            Game_Events_Manager.Instance.HarvestRequirementsMet();
            FinishQuestStep(); // Finish and destroy this object 
        }
            
    }
}
