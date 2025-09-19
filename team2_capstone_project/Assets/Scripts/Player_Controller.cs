using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Grimoire;
// using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    [HideInInspector] public Vector2 movement;

    // Input System
    private PlayerInput playerInput;
    private InputAction moveAction, interactAction, openInventoryAction, openJournalAction;

    // Room tracking
    [HideInInspector] public Room_Data currentRoom;

    // References
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        interactAction = playerInput.actions["Interact"];
        openInventoryAction = playerInput.actions["OpenInventory"];
        openJournalAction = playerInput.actions["OpenJournal"];
    }

    private void FixedUpdate()
    {
        movement = moveAction.ReadValue<Vector2>();
        rb.velocity = new Vector3(movement.x * moveSpeed, rb.velocity.y, movement.y * moveSpeed);
    }

    public void DisablePlayerController()
    {
        Player_Input_Controller.instance.DisablePlayerInput();
        rb.velocity = Vector3.zero;
    }

    public void EnablePlayerController()
    {
        Player_Input_Controller.instance.EnablePlayerInput();
    }

    public void UpdatePlayerRoom(Room_Data.RoomID newRoomID)
    {
        Room_Data newRoom = Room_Manager.GetRoom(newRoomID);
        if (newRoom != null)
        {
            currentRoom = newRoom;
        }
        else
        {
            Debug.LogWarning($"Room not found: {newRoomID}");
        }
    }
}
