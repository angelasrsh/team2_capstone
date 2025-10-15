using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Journal_Recipe_Quest_Step : Dialogue_Quest_Step
{
    private bool clicked = false;
    [SerializeField] private Dish_Data DishToMake; // Which dish the player must click. Set to one-day blinding stew.
    void OnEnable()
    {
        Game_Events_Manager.Instance.onDishDetailsClick += RecipeClick;
        Game_Events_Manager.Instance.onEndDialogBox += endQuest;
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onDishDetailsClick -= RecipeClick;
        Game_Events_Manager.Instance.onEndDialogBox -= endQuest;
    }

    private void Start()
    {
        DelayedDialogue(0, 0, false);
    }


    private void RecipeClick(Dish_Data dishData)
    {
        if (dishData == DishToMake)
            clicked = true; // Finish and destroy this object
    }

    private void endQuest()
    {
        FinishQuestStep();
    }
}
