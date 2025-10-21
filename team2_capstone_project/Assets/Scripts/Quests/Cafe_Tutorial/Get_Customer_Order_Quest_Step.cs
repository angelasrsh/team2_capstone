using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Get_Customer_Order_Quest_Step : Dialogue_Quest_Step
{
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
        DelayedDialogue(1, 0);
        DelayedDialogue(10, 0, false, "Journal.Get_Order2"); // Second reminder if they still haven't gotten an order
    }

    private void GetCustomerOrder() => FinishQuestStep(); // Finish and destroy this object
}
