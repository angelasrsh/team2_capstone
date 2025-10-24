using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Leave_Bedroom_Quest_Step : Dialogue_Quest_Step
{
    
    // Start is called before the first frame update
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onRoomChange += LeaveBedroom;
        Choose_Menu_Items.OnDailyMenuSelected += BeginDialogue;

        //DelayedInstructionStart();

    }

    // Update is called once per frame
    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onRoomChange -= LeaveBedroom;
        Choose_Menu_Items.OnDailyMenuSelected -= BeginDialogue;

    }

    private void BeginDialogue(List<Dish_Data.Dishes> list)
    {
        DelayedDialogue(0, 0, false);
    }
    


    /// <summary>
    /// End quest when player exits to a non-main-menu scene
    /// </summary>
    /// <param name="currentRoom"> RoomID of the room being left </param>
    /// <param name="exitingTo"> RoomID of the room being entered </param>
    void LeaveBedroom(Room_Data.RoomID currentRoom, Room_Data.RoomID exitingTo)
    {
        if (currentRoom == Room_Data.RoomID.Bedroom && (exitingTo != Room_Data.RoomID.Main_Menu))
            FinishQuestStep();

    }
}
