using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Get_Customer_Order_Quest_Step : Dialogue_Quest_Step
{

    private Customer_Spawner customerSpawner;
    public CustomerData customerToSpawn;
    public string key = "Asper.Get_Order_Asper";
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onGetOrder += GetCustomerOrder;
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onGetOrder -= GetCustomerOrder;
    }

    private void Start()
    {
        Customer_Spawner customerSpawner = GameObject.FindObjectOfType<Customer_Spawner>();
        if (customerSpawner == null) 
            Helpers.printLabeled(this, "No Customer_Spawner found in scene");
        else
        {
            customerSpawner.SpawnSingleCustomer(customerToSpawn);
        }


        DelayedDialogue(0, 0, false, key);
        DelayedDialogue(2, 0, false, "Journal.Leave_Tutorial");
        DelayedDialogue(10, 0, false, "Journal.Get_Order2"); // Second reminder if they still haven't gotten an order
    }

    private void GetCustomerOrder() => FinishQuestStep(); // Finish and destroy this object
}

// disable and enable restaurant spawning?