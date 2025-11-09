using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Room_Change_Interact : MonoBehaviour
{
    [Header("Room Transition Settings")]
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;
    public bool BlockIfInventoryFull;

    private bool isPlayerInRange = false;
    private bool interactPressed = false;
    private Player_Controller player;
    private InputAction interactAction;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRebind;
        TryBindInput();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedRebind;
        UnbindInput();
    }

    private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode)
    {
        TryBindInput();
        interactPressed = false; // reset after load
    }

    private void TryBindInput()
    {
        PlayerInput playerInput = null;

        // Preferred: pull from Game_Manager
        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        // Fallback: find any PlayerInput in the scene
        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[Room_Change_Interact] No PlayerInput found to bind 'Interact' action.");
            return;
        }

        interactAction = playerInput.actions["Interact"];
        if (interactAction != null)
        {
            interactAction.performed += OnInteractPerformed;
            interactAction.Enable();
            Debug.Log("[Room_Change_Interact] Bound to 'Interact' input successfully.");
        }
        else
        {
            Debug.LogWarning("[Room_Change_Interact] Could not find 'Interact' action in PlayerInput.");
        }
    }

    private void UnbindInput()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
            interactAction = null;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        interactPressed = true;
    }

   private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            player = other.GetComponent<Player_Controller>();

            // Reset input buffer to prevent accidental activation
            interactPressed = false;
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

            // Block if inventory is full and setting is true
            if (BlockIfInventoryFull && Dish_Tool_Inventory.Instance.IsFull())
            {
                Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();

                if (dm != null)
                    dm.PlayScene("Journal.Hands_Full");
                else
                    Helpers.printLabeled(this, "Dialogue manager is null");
                    
                return;
            }

            if (player != null)
            {
                Player_Input_Controller.instance.DisablePlayerInput();

                // Auto-save before room change
                if (Save_Manager.instance != null)
                {
                    Save_Manager.instance.AutoSave();
                    Debug.Log("Game auto-saved before room transition.");
                }

                Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
            }
        }
    }

}
