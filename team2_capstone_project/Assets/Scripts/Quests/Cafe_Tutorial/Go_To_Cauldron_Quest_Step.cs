using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Go_To_Cauldron_Quest_Step : Dialogue_Quest_Step
{
    public Dish_Data DishToMake;
    protected override void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
    }

    protected override void OnDisable()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    private void Start()
    {
        DelayedDialogue();
        //StartRepeatPrompt();
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Updated_Restaurant")
        {
            if (Dish_Tool_Inventory.Instance.HasItem(DishToMake))
            {
                FinishQuestStep();
            } else
                DelayedDialogue();
        }
            
    }
}
