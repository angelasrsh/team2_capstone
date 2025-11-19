
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Serve_Customer_Quest_Step : Dialogue_Quest_Step
{

    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onServeCustomer += ServeCustomer;
        base.OnEnable();

    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onServeCustomer -= ServeCustomer;
        base.OnDisable();
    }

    void Start()
    {
        DelayedDialogue(0, 0, false);
    }

    private void ServeCustomer()
    {
        Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Honey_Jelly_Drink);
        Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Honey_Glazed_Eleonoras);
        Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Boba_Milk_Drink);
        DelayedDialogue(0, 0, false, "Journal.End_Day");
        FinishQuestStep(); // Finish and destroy this object
    }
}
