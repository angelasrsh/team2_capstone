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
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;

    private Player_Controller player;
    private bool confirmationActive = false;
    private InputAction pauseAction;

    private void Awake()
    {
        leaveResourceAreaCanvas.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show confirmation UI
            leaveResourceAreaCanvas.enabled = true;
            confirmationActive = true;

            // Disable player movement and pausing while confirmation is up
            player = other.GetComponent<Player_Controller>();
            if (player != null)
                player.DisablePlayerMovement();

            Pause_Menu.instance.SetCanPause(false);

            // Disable pause input action directly
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
            {
                var input = pic.GetComponent<PlayerInput>();
                if (input != null)
                {
                    pauseAction = input.actions["Pause"];
                    if (pauseAction.enabled)
                        pauseAction.Disable();
                }
            }
        }
    }

    private void Update()
    {
        if (!confirmationActive)
            return;

        // Handle key input only while confirmation UI is active
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ClickYes();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClickNo();
        }
    }

    public void ClickYes()
    {
        Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
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

        Pause_Menu.instance.SetCanPause(true);

        if (player != null)
            player.EnablePlayerMovement();

        // Re-enable pause input after closing confirmation
        if (pauseAction != null && !pauseAction.enabled)
            pauseAction.Enable();
    }
}
