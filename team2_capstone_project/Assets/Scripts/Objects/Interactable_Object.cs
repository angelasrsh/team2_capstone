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
        // public event Action<float> OnHoldProgress;  // later for hold-to-interact UI timer

        [Header("Interaction Settings")]
        [SerializeField] protected float holdTime = 1f;
        protected float holdTimer;
        protected bool isHolding = false;

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
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInside = true;
                player = other.GetComponent<Player_Controller>();

                if (InteractIcon != null)
                    InteractIcon.SetActive(true);
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
            }
        }

        protected virtual void Update()
        {
            if (!playerInside || interactAction == null)
                return;

            // Holding logic
            if (interactAction.IsPressed())
            {
                if (!isHolding)
                {
                    isHolding = true;
                    holdTimer = 0f;
                }

                holdTimer += Time.deltaTime;

                if (holdTimer >= holdTime)
                {
                    PerformInteract();
                    ResetHold();
                }
            }
            else if (isHolding)
            {
                ResetHold();
                // OnHoldProgress?.Invoke(holdTimer / holdTime);  // later for hold-to-interact UI timer
            }
        }

        protected void ResetHold()
        {
            isHolding = false;
            holdTimer = 0f;
        }

        public virtual void PerformInteract()
        {
            Debug.Log($"[Interactable_Object] Player interacted with {name} (base)");
        }
    }
}

