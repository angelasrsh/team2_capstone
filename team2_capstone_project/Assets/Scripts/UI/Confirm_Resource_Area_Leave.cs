using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confirm_Resource_Area_Leave : MonoBehaviour
{
    public Canvas leaveResourceAreaCanvas;
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;
    private Player_Controller player;

    public void Awake()
    {
        leaveResourceAreaCanvas.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            leaveResourceAreaCanvas.enabled = true;
            Player_Input_Controller.instance.DisablePlayerInput();
        }
    }

    public void ClickYes()
    {
        Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
        leaveResourceAreaCanvas.enabled = false;
    }

    public void ClickNo()
    {
        leaveResourceAreaCanvas.enabled = false;
        Player_Input_Controller.instance.EnablePlayerInput();
    }
}