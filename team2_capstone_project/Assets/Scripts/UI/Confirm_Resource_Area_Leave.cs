using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;

public class Confirm_Resource_Area_Leave : MonoBehaviour
{
    [Header("References")]
    public Canvas leaveResourceAreaCanvas;
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;

    private Player_Controller player;
    private InputAction interactAction, pauseAction;
    private bool confirmationActive = false;

    private void Awake()
    {
        leaveResourceAreaCanvas.enabled = false;

        // Subscribe to scene changes to rebind input
        SceneManager.sceneLoaded += OnSceneLoadedRebind;

        TryBindInput();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRebind;
        TryBindInput();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedRebind;
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
    }
    
    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoadedRebind;
    private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode) => TryBindInput();
    private void TryBindInput()
    {
        PlayerInput playerInput = null;

        // Prefer Game_Manager if it holds PlayerInput
        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        // Fallback to Player_Input_Controller if needed
        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[Confirm_Resource_Area_Leave] No PlayerInput found in scene to bind actions.");
            return;
        }

        // Bind Interact action
        interactAction = playerInput.actions["Interact"];
        if (interactAction == null)
        {
            Debug.LogWarning("[Confirm_Resource_Area_Leave] Could not find 'Interact' action in PlayerInput!");
        }
        else
            interactAction.Enable();

        // Bind Pause action
        pauseAction = playerInput.actions["Pause"];
        if (pauseAction == null)
        {
            Debug.LogWarning("[Confirm_Resource_Area_Leave] Could not find 'Pause' action in PlayerInput!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        player = other.GetComponent<Player_Controller>();

        // Enable the action listener when inside the trigger
        if (interactAction != null)
        {
            interactAction.performed += OnInteractPerformed;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Remove listener when player leaves
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;

        player = null;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (confirmationActive) return;
        if (player == null) return;

        OpenConfirmation(player);
    }

    private void OpenConfirmation(Player_Controller playerController)
    {
        ///////////// Specific exiting cases
        
        if (SceneManager.GetActiveScene().name == "Foraging_Area_Whitebox")
            leaveResourceAreaCanvas.GetComponent<Leave_Resource_Area_Canvas_Script>().SetText();

        // Case: Don't go outside if it's late
        if (SceneManager.GetActiveScene().name == "Updated_Restaurant" && exitingTo == Room_Data.RoomID.Foraging_Area_Whitebox
            && Day_Turnover_Manager.Instance.currentTimeOfDay != Day_Turnover_Manager.TimeOfDay.Morning)
        {
            Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
            dm.PlayScene("Default.ExitDoor");
            return;
        }
        /////////////
            
            
        leaveResourceAreaCanvas.enabled = true;
        confirmationActive = true;

        player = playerController;
        if (player != null)
            player.DisablePlayerMovement();

        Pause_Menu.instance?.SetCanPause(false);

        if (pauseAction != null && pauseAction.enabled)
            pauseAction.Disable();
    }

    private void Update()
    {
        if (!confirmationActive)
            return;

        // Handle Enter and Escape for Yes/No
        if (Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                ClickYes();
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                ClickNo();
        }
    }

    public void ClickYes()
    {
        if (!confirmationActive)
            return;
            
        Room_Change_Manager.instance?.GoToRoom(currentRoom.roomID, exitingTo);
        CloseConfirmation();
        player.DisablePlayerMovement();
    }

    public void ClickNo()
    {
        CloseConfirmation();
    }

    private void CloseConfirmation()
    {
        leaveResourceAreaCanvas.enabled = false;
        confirmationActive = false;

        Pause_Menu.instance?.SetCanPause(true);

        if (player != null)
            player.EnablePlayerMovement();

        if (pauseAction != null && !pauseAction.enabled)
            pauseAction.Enable();
    }
}
