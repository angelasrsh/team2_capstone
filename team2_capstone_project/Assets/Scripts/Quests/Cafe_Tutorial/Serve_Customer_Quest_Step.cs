
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Serve_Customer_Quest_Step : Dialogue_Quest_Step
{

    private void Start()
    {
        // Delayed subscription, ensures this step is fully active
        StartCoroutine(SubscribeNextFrame());
    }

    private IEnumerator SubscribeNextFrame()
    {
        yield return null; // wait one frame so QuestManager doesn't destroy it immediately
        Debug.Log("[ServeQuestStep] Subscribing to onServeCustomer after initialization");
        Game_Events_Manager.Instance.onServeCustomer += ServeCustomer;
        
        // Now run the tutorial dialogue 
        DelayedDialogue(0, 0, false);
    }

    protected override void OnDisable()
    {
        Debug.Log("[ServeQuestStep] OnDisable unsubscribing...");
        if (Game_Events_Manager.Instance != null)
            Game_Events_Manager.Instance.onServeCustomer -= ServeCustomer;

        base.OnDisable();
    }

    private void Update()
    {
        Debug.Log("[ServeQuestStep] I am alive in Update");
    }

    private void ServeCustomer()
    {
        // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Honey_Jelly_Drink);
        // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Honey_Glazed_Eleonoras);
        // Player_Progress.Instance.UnlockDish(Dish_Data.Dishes.Boba_Milk_Drink);

        Player_Progress.Instance.SetGameplayTutorial(false);
        Debug.Log("Tutorial mode disabled in Serve_Customer_Quest_Step");
        DelayedDialogue(0, 0, false, "Journal.End_Day");
        FinishQuestStep(); // Finish and destroy this object
    }
}
