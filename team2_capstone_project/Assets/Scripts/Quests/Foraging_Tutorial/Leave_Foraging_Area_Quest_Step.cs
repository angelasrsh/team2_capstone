using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Leave_Foraging_Area_Quest_Step : Dialogue_Quest_Step
{

    // Start is called before the first frame update
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onRoomChange += LeaveForagingArea;

    }

    // Update is called once per frame
    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onRoomChange -= LeaveForagingArea;
    }

    void Start()
    {
        StartCoroutine(PromptLeaveResourceArea());
    }

    private IEnumerator PromptLeaveResourceArea()
    {
        yield return new WaitForSeconds(20);
        Room_Data room = Room_Manager.GetRoomFromActiveScene();

        // If in whitebox, prompt creturning to the carriage to exit
        if (room.roomID == Room_Data.RoomID.Foraging_Area_Whitebox)
            DelayedDialogue(0, 0, false);
        
    }



    /// <summary>
    /// End quest when player exits to a non-main-menu scene
    /// </summary>
    /// <param name="currentRoom"> RoomID of the room being left </param>
    /// <param name="exitingTo"> RoomID of the room being entered </param>
    void LeaveForagingArea(Room_Data.RoomID currentRoom, Room_Data.RoomID exitingTo)
    {
        if (exitingTo == Room_Data.RoomID.Outside_Restaurant)
            FinishQuestStep();

    }
}
