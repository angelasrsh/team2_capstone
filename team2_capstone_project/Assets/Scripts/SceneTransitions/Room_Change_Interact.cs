using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Room_Change_Interact : MonoBehaviour
{
    [Header("Room Transition Settings")]
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;

    private bool isPlayerInRange = false;
    private bool interactPressed = false;
    private Player_Controller player;
    private InputAction interactAction;

    private void Awake()
    {
        var playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interact"];
            if (interactAction != null)
                interactAction.performed += ctx => interactPressed = true;
        }
    }

    private void OnDestroy()
    {
        if (interactAction != null)
            interactAction.performed -= ctx => interactPressed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            player = other.GetComponent<Player_Controller>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            player = null;
        }
    }

    private void Update()
    {
        // Only process input once per press while player is inside trigger
        if (isPlayerInRange && interactPressed)
        {
            interactPressed = false; // consume input
            Debug.Log($"Player interacted to change room to: {exitingTo}");

            if (player != null)
            {
                Player_Input_Controller.instance.DisablePlayerInput();
                Game_Events_Manager.Instance.RoomChange(exitingTo);
                Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
            }
        }
    }
}
