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

    // private void Start()
    // {
    //     customerSpawner = FindObjectOfType<Customer_Spawner>();
    //     if (customerSpawner == null)
    //     {
    //         Helpers.printLabeled(this, "No Customer_Spawner found in scene");
    //         return;
    //     }

    //     // --- Prevent duplicate spawns if loading from save ---
    //     if (Restaurant_State.Instance != null)
    //     {
    //         bool alreadyExists = Restaurant_State.Instance.customers.Exists(c =>
    //             c.customerName == customerToSpawn.customerName
    //         );

    //         if (alreadyExists)
    //             Debug.Log($"Tutorial: Customer {customerToSpawn.customerName} already exists in Restaurant_State. Skipping spawn.");
    //         else
    //         {
    //             var cc = customerSpawner.SpawnSingleCustomerReturn(customerToSpawn);
    //             if (cc != null)
    //                 cc.isTutorialCustomer = true;
    //         }
    //     }
    //     else
    //     {
    //         var cc = customerSpawner.SpawnSingleCustomerReturn(customerToSpawn);
    //         if (cc != null)
    //             cc.isTutorialCustomer = true;
    //     }

    //     //DelayedDialogue(0, 0, false, key);

    //     // Reminder dialogues
    //     DelayedDialogue(2, 0, false, "Journal.Leave_Tutorial");
    //     DelayedDialogue(10, 0, false, "Journal.Get_Order2");
    // }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => Save_Manager.HasLoadedGameData);

        // Prevent double spawn after load
        if(Restaurant_State.Instance.customers.Exists(c => 
                c.customerName == customerToSpawn.customerName))
        {
            Debug.Log("Tutorial: customer already exists, skipping spawn.");
            yield break;
        }

        customerSpawner = FindObjectOfType<Customer_Spawner>();
        var cc = customerSpawner.SpawnSingleCustomerReturn(customerToSpawn);
        cc.isTutorialCustomer = true;

        DelayedDialogue(2, 0, false, "Journal.Leave_Tutorial");
        DelayedDialogue(10, 0, false, "Journal.Get_Order2");
    }

    private void GetCustomerOrder() => FinishQuestStep(); // Finish and destroy this object
}

// disable and enable restaurant spawning?