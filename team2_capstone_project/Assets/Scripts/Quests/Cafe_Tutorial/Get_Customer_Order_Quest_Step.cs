using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Get_Customer_Order_Quest_Step : Dialogue_Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onGetOrder += GetCustomerOrder;
        DelayedDialogue(1, 0);
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onGetOrder -= GetCustomerOrder;
    }


    private void GetCustomerOrder()
    {
        FinishQuestStep(); // Finish and destroy this object
    }
}
