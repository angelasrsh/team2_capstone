using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Serve_Customer_Quest_Step : Tutorial_Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onServeCustomer += ServeCustomer;
        DelayedInstructionStart();
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onServeCustomer -= ServeCustomer;
    }


    private void ServeCustomer()
    {
        FinishQuestStep(); // Finish and destroy this object
    }
}
