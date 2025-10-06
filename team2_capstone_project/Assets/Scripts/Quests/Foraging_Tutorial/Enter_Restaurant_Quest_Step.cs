using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enter_Restaurant_Quest_Step : Tutorial_Quest_Step
{

    [Header("Items to collect")]
    public Ingredient_Data[] RequiredIngredients;
    public int[] ItemCounts;

    void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
        Game_Events_Manager.Instance.onResourceAdd += ResourceAdd;

        
        if (ItemCounts.Length != RequiredIngredients.Length)
            Debug.Log($"{GetType().Name} on {gameObject.name} Error: Please ensure there is a required count for every item");
       

        DelayedInstructionStart();
      
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;
        Game_Events_Manager.Instance.onResourceAdd -= ResourceAdd;
    }
    
    private void ResourceAdd(Ingredient_Data ing)
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
        }
            
    }


    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Restaurant")
            FinishQuestStep();

    }
}
