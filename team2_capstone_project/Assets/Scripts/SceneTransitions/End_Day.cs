using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.InputSystem;

public class End_Day : MonoBehaviour
{
    private bool interactPressed;
    private Room_Change_Trigger leaveTrigger;

    private void Awake()
    {
        var playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            InputAction interactAction = playerInput.actions["Interact"];
            interactAction.performed += ctx => interactPressed = true;
        }

        FindTimeofDay();
    }

    private void FindTimeofDay()
    {
        leaveTrigger = FindObjectOfType<Room_Change_Trigger>();
        if (leaveTrigger == null)
        {
            Debug.LogWarning("[End_Day] No Room_Change_Trigger found in scene.");
        }

        if (Day_Turnover_Manager.Instance.currentTimeOfDay == Day_Turnover_Manager.TimeOfDay.Evening)
        {
            if (leaveTrigger != null)
            {
                leaveTrigger.gameObject.SetActive(false);
                Debug.Log($"[End_Day] Disabled {leaveTrigger.gameObject.name} for evening.");
            }
        }
        else
        {
            if (leaveTrigger != null)
            {
                leaveTrigger.gameObject.SetActive(true);
                Debug.Log($"[End_Day] Enabled {leaveTrigger.gameObject.name} for morning.");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && interactPressed)
        {
            interactPressed = false;

            if (Day_Turnover_Manager.Instance.currentTimeOfDay == Day_Turnover_Manager.TimeOfDay.Evening)
            {
                Day_Turnover_Manager.Instance.EndDay();
                Audio_Manager.instance.PlaySFX(Audio_Manager.instance.goToBed, 0.35f);
                leaveTrigger.gameObject.SetActive(true);  // Re-enable the trigger for next morning
            }
            else
            {
                Debug.Log($"[End_Day] Cannot end day yet. TimeOfDay = {Day_Turnover_Manager.Instance.currentTimeOfDay}");
            }
        }
    }
}
