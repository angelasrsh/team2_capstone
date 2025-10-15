
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Serve_Customer_Quest_Step : Dialogue_Quest_Step
{
    public Canvas ticket;
    void OnEnable()
    {
        Game_Events_Manager.Instance.onServeCustomer += ServeCustomer;
        ticket = GameObject.Find("Temp_Recipe_Canvas").GetComponent<Canvas>();

        DelayedDialogue(1, 0);
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onServeCustomer -= ServeCustomer;
    }

   

    private void ServeCustomer()
    {
        ticket.gameObject.SetActive(false);
        FinishQuestStep(); // Finish and destroy this object
    }
}
