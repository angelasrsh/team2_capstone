using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Piano : MonoBehaviour
{
    private InputAction interactAction;
    private bool playerInsideTrigger;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRebind;
        TryBindInput();
    }

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoadedRebind;
    private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode) => TryBindInput();

    private void TryBindInput()
    {
        PlayerInput playerInput = null;

        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[Piano] No PlayerInput found to bind actions.");
            return;
        }

        interactAction = playerInput.actions["Interact"];
        if (interactAction == null)
        {
            Debug.LogWarning("[Piano] Could not find 'Interact' action in PlayerInput!");
            return;
        }

        interactAction.performed += ctx => TryPlayPianoNote();
        interactAction.Enable();

        Debug.Log("[Piano] Input bound successfully.");
    }

    private void TryPlayPianoNote()
    {
        if (playerInsideTrigger)
            Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.pianoHit, 0.75f, Random.Range(0.8f, 1.2f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInsideTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInsideTrigger = false;
    }
}
