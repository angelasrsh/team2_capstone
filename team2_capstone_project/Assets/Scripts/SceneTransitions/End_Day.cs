using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class End_Day : MonoBehaviour
{
    private bool interactPressed;

    private void Awake()
    {
        var playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            InputAction interactAction = playerInput.actions["Interact"];
            interactAction.performed += ctx => interactPressed = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && interactPressed)
        {
            interactPressed = false; // consume input to prevent spamming
            Debug.Log("Player interacted to end day.");

            Day_Turnover_Manager.Instance.EndDay();
        }
    }
}
