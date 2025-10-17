using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class End_Day : MonoBehaviour
{
    private bool interactPressed;
    private InputAction interactAction;
    private Room_Change_Trigger leaveTrigger;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRebind;
        TryBindInput();
        FindTimeofDay();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedRebind;
        UnbindInput();
    }

    private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode)
    {
        TryBindInput();
        FindTimeofDay();
    }

    private void TryBindInput()
    {
        PlayerInput playerInput = null;

        // Preferred: pull from Game_Manager
        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        // Fallback: find via Player_Input_Controller
        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[End_Day] No PlayerInput found when trying to bind Interact action.");
            return;
        }

        interactAction = playerInput.actions["Interact"];
        if (interactAction != null)
        {
            interactAction.performed += OnInteractPerformed;
            interactAction.Enable();
            Debug.Log("[End_Day] Bound to 'Interact' input successfully.");
        }
        else
        {
            Debug.LogWarning("[End_Day] 'Interact' action not found in PlayerInput.");
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

    private void FindTimeofDay()
    {
        leaveTrigger = FindObjectOfType<Room_Change_Trigger>(true);
        if (leaveTrigger == null)
        {
            Debug.LogWarning("[End_Day] No Room_Change_Trigger found in scene.");
            return;
        }

        if (Day_Turnover_Manager.Instance.currentTimeOfDay == Day_Turnover_Manager.TimeOfDay.Evening)
        {
            leaveTrigger.gameObject.SetActive(false);
            Debug.Log($"[End_Day] Disabled {leaveTrigger.gameObject.name} for evening.");
        }
        else
        {
            leaveTrigger.gameObject.SetActive(true);
            Debug.Log($"[End_Day] Enabled {leaveTrigger.gameObject.name} for morning.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && interactPressed)
        {
            interactPressed = false; // consume input

            if (Day_Turnover_Manager.Instance.currentTimeOfDay == Day_Turnover_Manager.TimeOfDay.Evening)
            {
                Debug.Log("[End_Day] Ending day...");
                Day_Turnover_Manager.Instance.EndDay();
                Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.goToBed, 0.35f);

                if (leaveTrigger != null)
                {
                    // Debug.Log("[End_Day]: re-enabled leave trigger");
                    leaveTrigger.gameObject.SetActive(true); // Re-enable for next morning
                }
            }
            else
            {
                Debug.Log($"[End_Day] Cannot end day yet. TimeOfDay = {Day_Turnover_Manager.Instance.currentTimeOfDay}");
            }
        }
    }
}
