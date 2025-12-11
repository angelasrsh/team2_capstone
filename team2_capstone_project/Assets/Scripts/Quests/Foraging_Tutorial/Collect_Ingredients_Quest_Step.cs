using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Collect_Ingredients_Quest_Step : Dialogue_Quest_Step
{
    [Header("Items to collect")]
    public Ingredient_Data[] RequiredIngredients;
    public int[] ItemCounts;

    protected override void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
        Game_Events_Manager.Instance.onResourceAdd += ResourceAdd;
        Game_Events_Manager.Instance.onPlayerSprint += OnPlayerSprint; 

        if (ItemCounts.Length != RequiredIngredients.Length)
            Debug.Log($"{GetType().Name} on {gameObject.name} Error: Please ensure there is a required count for every item");

        DelayedDialogue(0, 0, false);
    }

    protected override void OnDisable()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;
        Game_Events_Manager.Instance.onResourceAdd -= ResourceAdd;
        Game_Events_Manager.Instance.onPlayerSprint -= OnPlayerSprint; // 👈 Unsubscribe
    }

    private void Start()
    {
        checkRequirementsMet();
    }

    private void ResourceAdd(Ingredient_Data ing)
    {
        checkRequirementsMet();
        // FinishQuestStep();
    }

    private void OnPlayerSprint()
    {
        Debug.Log("[Tutorial] Player sprinted!");

        if (dm != null)
            dm.EndDialog();
        FinishQuestStep();
    }

    private void checkRequirementsMet()
    {
        // bool hasAllItems = true;
        // for (int i = 0; i < RequiredIngredients.Length; i++)
        // {
        //     if (Ingredient_Inventory.Instance.GetItemCount(RequiredIngredients[i]) < ItemCounts[i])
        //     {
        //         hasAllItems = false;
        //         break;
        //     }
        // }

        // if (hasAllItems)
        // {
        //     Game_Events_Manager.Instance.HarvestRequirementsMet();
        //     FinishQuestStep();
        // }
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Updated_Restaurant")
            FinishQuestStep();
    }
}
