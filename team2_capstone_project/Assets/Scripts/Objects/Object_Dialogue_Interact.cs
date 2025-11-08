using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Object_Dialogue_Interact : MonoBehaviour
{
    [Header("Dialogue Interaction Key")]
    public string dialogKey;

    private InputAction interactAction;
    private bool playerInsideTrigger;
    private Dialogue_Manager dm;

    private void Start()
    {
        dm = FindObjectOfType<Dialogue_Manager>();
    }

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
            Debug.LogWarning("[Object_Dialogue_Interact] No PlayerInput found to bind actions.");
            return;
        }

        interactAction = playerInput.actions["Interact"];
        if (interactAction == null)
        {
            Debug.LogWarning("[Object_Dialogue_Interact] Could not find 'Interact' action in PlayerInput!");
            return;
        }

        interactAction.performed += ctx => PlayDialogInteraction();
        interactAction.Enable();

        Debug.Log("[Object_Dialogue_Interact] Input bound successfully.");
    }

    private void PlayDialogInteraction()
    {
        if (playerInsideTrigger)
            dm.PlayScene(dialogKey);
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
