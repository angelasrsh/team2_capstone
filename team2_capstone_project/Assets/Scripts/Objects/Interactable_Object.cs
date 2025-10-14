using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Grimoire
{
    public class Interactable_Object : MonoBehaviour
    {
        [Header("UI")]
        public GameObject InteractIcon;

        [Header("Interaction Settings")]
        [SerializeField] protected float holdTime = 1f;
        protected float holdTimer;
        protected bool isHolding = false;

        protected bool playerInside = false;
        protected PlayerInput playerInput;
        protected InputAction interactAction;
        protected Player_Controller player;

        public static event Action<float> OnGlobalHoldProgress;  // event for UI fill

        protected virtual void Awake()
        {
            if (InteractIcon == null)
            {
                var icon = transform.Find("Interact_Icon");
                if (icon != null)
                    InteractIcon = icon.gameObject;
            }

            SceneManager.sceneLoaded += OnSceneLoadedRebind;
        }

        protected virtual void OnEnable()
        {
            TryBindInput();
            SceneManager.sceneLoaded += OnSceneLoadedRebind;
        }

        protected virtual void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedRebind;
        }

        private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode)
        {
            TryBindInput();
        }

        protected virtual void TryBindInput()
        {
            playerInput = Game_Manager.Instance?.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogWarning($"[{name}] No PlayerInput found when trying to bind interact input.");
                return;
            }

            interactAction = playerInput.actions["Interact"];
            if (interactAction == null)
            {
                Debug.LogWarning($"[{name}] No 'Interact' action found in PlayerInput.");
                return;
            }

            interactAction.Enable(); // Keep it always enabled
        }

        protected virtual void Update()
        {
            if (!playerInside || interactAction == null)
                return;

            if (interactAction.IsPressed())
            {
                if (!isHolding)
                {
                    isHolding = true;
                    holdTimer = 0f;
                }

                holdTimer += Time.deltaTime;

                float progress = Mathf.Clamp01(holdTimer / holdTime);
                OnGlobalHoldProgress?.Invoke(progress);

                if (holdTimer >= holdTime)
                {
                    PerformInteract();
                    ResetHold();
                    OnGlobalHoldProgress?.Invoke(0f); // reset fill
                }
            }
            else if (isHolding)
            {
                ResetHold();
                OnGlobalHoldProgress?.Invoke(0f);
            }
        }

        protected void ResetHold()
        {
            isHolding = false;
            holdTimer = 0f;
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

                OnGlobalHoldProgress?.Invoke(0f);
            }
        }

        public virtual void PerformInteract()
        {
            Debug.Log($"[Interactable_Object] Player interacted with {name}");
        }
    }
}
