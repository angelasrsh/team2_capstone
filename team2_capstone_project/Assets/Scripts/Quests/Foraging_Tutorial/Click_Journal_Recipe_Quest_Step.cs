using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Journal_Recipe_Quest_Step : Tutorial_Quest_Step
{
    public Dish_Data DishToMake; // Which dish the player must click
    void OnEnable()
    {
        Game_Events_Manager.Instance.onDishDetailsClick += RecipeClick;
        DelayedInstructionStart();

        if (DishToMake == null)
            Debug.Log($"{GetType().Name} on {name} error: DishToMake is null. Please assign it in the inspector.");
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onDishDetailsClick -= RecipeClick;
    }


    private void RecipeClick(Dish_Data dishData)
    {
        if (dishData == DishToMake)
            FinishQuestStep(); // Finish and destroy this object
    }
}
