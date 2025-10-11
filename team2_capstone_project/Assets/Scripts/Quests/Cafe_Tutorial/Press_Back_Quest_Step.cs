using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Press_Back_Quest_Step : Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onRoomChange += RoomChange;
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onRoomChange -= RoomChange;
    }


    private void RoomChange(Room_Data.RoomID newRoom)
    {
        if (newRoom == Room_Data.RoomID.Restaurant)
            FinishQuestStep(); // Finish and destroy this object
    }
}
