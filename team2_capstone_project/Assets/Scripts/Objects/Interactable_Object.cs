using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Grimoire
{
    /// <summary>
    /// Base class for any objects that need to do something when the player is in range.
    /// Must be on an object with a trigger collider.
    /// Derived classes can override the PerformInteract() function to add their own reponses.
    /// </summary>
    public class Interactable_Object : MonoBehaviour
    {
        public GameObject InteractIcon; // Not sure if this is really needed
        private bool playerInside = false;
        private PlayerInput playerInput;
        private InputAction interactAction;

        void Awake()
        {
            if (InteractIcon == null)
            {
                InteractIcon = transform.Find("Interact_Icon").gameObject;
            }

            // New input system (to allow for PC + mobile controls)
            playerInput = FindObjectOfType<PlayerInput>();
            interactAction = playerInput.actions["Interact"];
        }

        void OnTriggerEnter(Collider other)
        {
            // show interact option
            if (other.gameObject.CompareTag("Player"))
            {
                if (InteractIcon != null)
                    InteractIcon.SetActive(true);
                playerInside = true;
            }

        }

        void Update()
        {
            if (playerInside && interactAction.triggered)
            {
                PerformInteract();
            }

        }

        void OnTriggerExit(Collider other)
        {
            // hide interact option
            if (other.gameObject.CompareTag("Player"))
            {
                if (InteractIcon != null)
                    InteractIcon.SetActive(false);
                playerInside = false;
            }

        }

        /// <summary>
        /// Action that happens when player presses 'E' to interact while nearby.
        /// </summary>
        public virtual void PerformInteract()
        {
            Debug.Log($"[IntObj] Player interacted with " + gameObject.ToString());
        }
    }

}
