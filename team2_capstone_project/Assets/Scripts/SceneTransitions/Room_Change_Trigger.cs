using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Room_Change_Trigger : MonoBehaviour
{
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;
    private Player_Controller player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger for room change: " + exitingTo);
            Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
            Game_Events_Manager.Instance.RoomChange(exitingTo);
        }
    }
    
    public void OnButtonPressGoToRoom()
    {
        Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
    }
}