using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Room_Change_Interact : MonoBehaviour
{
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;

    private bool interactPressed;

    private void Awake()
    {
        var playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            InputAction interactAction = playerInput.actions["Interact"];
            interactAction.performed += ctx => interactPressed = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && interactPressed)
        {
            interactPressed = false; // consume input to prevent spamming

            Debug.Log("Player interacted to change room to: " + exitingTo);

            var player = other.GetComponent<Player_Controller>();
            if (player != null)
            {
                Player_Input_Controller.instance.DisablePlayerInput();
                // player.DisablePlayerController();
                Game_Events_Manager.Instance.RoomChange(exitingTo);
                Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
            }
        }
    }
}
