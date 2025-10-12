using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Leave_Bedroom_Quest_Step : Tutorial_Quest_Step
{
    Dialogue_Manager dm;

    // Start is called before the first frame update
    void OnEnable()
    {
        Game_Events_Manager.Instance.onRoomChange += LeaveBedroom;

        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
        if (dm != null)
        {
            string fillerKey = $"Tutorial.Bedroom";
            dm.PlayScene(fillerKey, CustomerData.EmotionPortrait.Emotion.Neutral);
        }

        //DelayedInstructionStart();

    }

    // Update is called once per frame
    void OnDisable()
    {
        Game_Events_Manager.Instance.onRoomChange -= LeaveBedroom;

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
