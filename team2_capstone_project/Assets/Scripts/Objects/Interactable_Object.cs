using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Grimoire
{
    /// <summary>
    /// Base class for any interactable world object (e.g., collectibles, doors, NPCs).
    /// Detects when the player is in range and listens for the Interact input.
    /// </summary>
    public class Interactable_Object : MonoBehaviour
    {
        [Header("UI")]
        public GameObject InteractIcon;

        protected bool playerInside = false;
        protected PlayerInput playerInput;
        protected InputAction interactAction;
        protected Player_Controller player;

        protected virtual void Awake()
        {
            if (InteractIcon == null)
            {
                var icon = transform.Find("Interact_Icon");
                if (icon != null)
                    InteractIcon = icon.gameObject;
            }

            playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("[Interactable_Object] No PlayerInput found in scene!");
                return;
            }

            interactAction = playerInput.actions["Interact"];
            if (interactAction == null)
            {
                Debug.LogError("[Interactable_Object] No 'Interact' action found in PlayerInput!");
            }
            else
            {
                Debug.Log("[Interactable_Object] Interact action found.");
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInside = true;
                player = other.GetComponent<Player_Controller>();

                if (InteractIcon != null)
                    InteractIcon.SetActive(true);

                Debug.Log($"[Interactable_Object] Player entered {name}");
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInside = false;
                player = null;

                if (InteractIcon != null)
                    InteractIcon.SetActive(false);

                Debug.Log($"[Interactable_Object] Player exited {name}");
            }
        }

        protected virtual void Update()
        {
            if (!playerInside || interactAction == null)
                return;

            if (interactAction.triggered)
            {
                PerformInteract();
            }
        }

        public virtual void PerformInteract()
        {
            Debug.Log($"[Interactable_Object] Player interacted with {name} (base)");
        }
    }
}
