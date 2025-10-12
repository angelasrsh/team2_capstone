using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Room_Change_Interact : MonoBehaviour
{
    [Header("Room Transition Settings")]
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;

    private bool isPlayerInRange = false;
    private bool interactTriggered = false;
    private Player_Controller player;
    private InputAction interactAction;

    private void Awake()
    {
        // Get PlayerInput safely (from GameManager or Player)
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interact"];

            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
                interactAction.Enable();
                Debug.Log("[Room_Change_Interact] Interact action bound.");
            }
            else
            {
                Debug.LogWarning("[Room_Change_Interact] 'Interact' action not found in PlayerInput.");
            }
        }
        else
        {
            Debug.LogWarning("[Room_Change_Interact] PlayerInput not found in scene.");
        }
    }

    private void OnDestroy()
    {
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (isPlayerInRange)
            interactTriggered = true;
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
        if (isPlayerInRange && interactTriggered)
        {
            interactTriggered = false; // consume input

            Debug.Log($"[Room_Change_Interact] Player interacted to change room to: {exitingTo}");

            if (player != null)
            {
                Game_Events_Manager.Instance.RoomChange(exitingTo);
                Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
            }
        }
    }
}
