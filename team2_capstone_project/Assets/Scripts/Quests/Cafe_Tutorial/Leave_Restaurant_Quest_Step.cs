using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Leave_Restaurant_Quest_Step : Dialogue_Quest_Step
{
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
        if (scene.name == "Bedroom")
        {
            // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Honey_Jelly_Drink);
            // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Honey_Glazed_Eleonoras);
            // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Boba_Milk_Drink);

            FinishQuestStep();
        }
            
    }
}
