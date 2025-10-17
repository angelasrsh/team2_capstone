using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enter_Restaurant_Quest_Step : Dialogue_Quest_Step
{

    // Start is called before the first frame update
    void OnEnable()
    {
        Game_Events_Manager.Instance.onRoomChange += EnterRestaurant;
        // make sure to nblock area
        DelayedDialogue(0, 0, false);

    }

    // Update is called once per frame
    void OnDisable()
    {
        Game_Events_Manager.Instance.onRoomChange -= EnterRestaurant;

    }

    void Start()
    {
        Game_Events_Manager.Instance.SetExitsBlocked(false);
    }



    /// <summary>
    /// End quest when player exits to a non-main-menu scene
    /// </summary>
    /// <param name="currentRoom"> RoomID of the room being left </param>
    /// <param name="exitingTo"> RoomID of the room being entered </param>
    void EnterRestaurant(Room_Data.RoomID currentRoom, Room_Data.RoomID exitingTo)
    {
        if (currentRoom == Room_Data.RoomID.Foraging_Area_Whitebox && (exitingTo != Room_Data.RoomID.Restaurant))
            FinishQuestStep();

    }
}
