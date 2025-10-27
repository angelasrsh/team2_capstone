using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// delete when ticket system is implemented
public class Temp_Ticket : MonoBehaviour
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onQuestStateChange += disableTicket;

    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onQuestStateChange -= disableTicket;
    }

    void Start()
    {
        if (Quest_Manager.Instance.GetQuestByID("Restaurant_Tutorial").state == Quest_State.FINISHED)
            this.enabled = false;

    }

    void disableTicket(Quest q)
    {
        if (q.Info.id == "Restaurant_Tutorial" && q.state == Quest_State.FINISHED)
            this.enabled = false;
        
    }
}
