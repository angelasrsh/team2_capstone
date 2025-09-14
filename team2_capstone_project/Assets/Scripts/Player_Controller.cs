using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Grimoire;
// using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour
{
    public Rigidbody rb;

    // Movement
    public float moveSpeed = 5f;
    float horizontalMovement;
    float verticalMovement;

    // Room tracking
    [HideInInspector] public Room_Data currentRoom;

    // Update is called once per frame
    void Update()
    {
        // velocity is updated by Input system callbacks
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(horizontalMovement * moveSpeed, rb.velocity.y, verticalMovement * moveSpeed);

    }

    // Called by Unity Input system
    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        verticalMovement = context.ReadValue<Vector2>().y;
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
