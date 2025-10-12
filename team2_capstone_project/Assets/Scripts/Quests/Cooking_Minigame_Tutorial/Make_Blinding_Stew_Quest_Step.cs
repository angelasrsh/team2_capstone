using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Make_Blinding_Stew_Quest_Step : Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onDishAdd += DishAdd;
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onDishAdd -= DishAdd;
    }


    private void DishAdd(Dish_Data dish)
    {
        if (dish.dishType == Dish_Data.Dishes.Blinding_Stew)
            FinishQuestStep(); // Finish and destroy this object
    }
}
