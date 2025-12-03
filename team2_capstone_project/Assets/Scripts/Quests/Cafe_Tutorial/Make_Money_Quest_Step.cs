
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Make_Money_Quest_Step : Dialogue_Quest_Step
{

    protected override void OnEnable() => base.OnEnable();
    protected override void OnDisable() => base.OnDisable();

    private void Start()
    {
        // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Honey_Jelly_Drink);
        // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Honey_Glazed_Eleonoras);
        // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Boba_Milk_Drink);

        QuestStepComplete = true; // Finish and destroy this object
        DelayedDialogue(0, 0, false);
    }
}
