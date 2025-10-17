using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Confirm_Resource_Area_Leave : MonoBehaviour
{
    [Header("References")]
    public Canvas leaveResourceAreaCanvas;
    public Canvas warningLeaveResourceAreaCanvas;
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;

    private Player_Controller player;
    private InputAction interactAction;
    private InputAction pauseAction;
    private bool confirmationActive = false;

    private void Awake()
    {
        leaveResourceAreaCanvas.enabled = false;
        if (warningLeaveResourceAreaCanvas != null)
            warningLeaveResourceAreaCanvas.enabled = false;

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
    }

    private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode)
    {
        TryBindInput();
    }

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
        {
            interactAction.Enable();
        }

        // Bind Pause action (we’ll disable it when the confirmation opens)
        pauseAction = playerInput.actions["Pause"];
        if (pauseAction == null)
        {
            Debug.LogWarning("[Confirm_Resource_Area_Leave] Could not find 'Pause' action in PlayerInput!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || interactAction == null)
            return;

        if (interactAction.triggered && !confirmationActive)
        {
            OpenConfirmation(other.GetComponent<Player_Controller>());
        }
    }

    private void OpenConfirmation(Player_Controller playerController)
    {
        if (!haveEnoughResources() && SceneManager.GetActiveScene().name == "Foraging_Area_Whitebox" && warningLeaveResourceAreaCanvas != null)
            warningLeaveResourceAreaCanvas.enabled = true; // Maybe just set text later?
        else
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
        Room_Change_Manager.instance?.GoToRoom(currentRoom.roomID, exitingTo);
        CloseConfirmation();
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

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedRebind;
    }

    /// <summary>
    /// Check if we have enough resources. Hard-coded to stew
    /// TODO: Check for all selected daily recipes
    /// </summary>
    private bool haveEnoughResources()
    {
        int numShrooms = Ingredient_Inventory.Instance.GetItemCount(IngredientType.Uncut_Fogshroom);
        int numEyes = Ingredient_Inventory.Instance.GetItemCount(IngredientType.Uncut_Fermented_Eye);
        int numBones = Ingredient_Inventory.Instance.GetItemCount(IngredientType.Bone);

        if (numShrooms >= Day_Plan_Manager.instance.customersPlannedForEvening
            && numEyes >= Day_Plan_Manager.instance.customersPlannedForEvening
            && numBones >= Day_Plan_Manager.instance.customersPlannedForEvening)
            return true;
        else
            return false;
    }
}
