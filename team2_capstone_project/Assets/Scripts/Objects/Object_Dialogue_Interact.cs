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

    private InputAction talkAction;
    private bool playerInsideTrigger;
    private Dialogue_Manager dm;
    private Dialog_UI_Manager dialogUIManager;

    private void Start()
    {
        dm = FindObjectOfType<Dialogue_Manager>();
        dialogUIManager = FindObjectOfType<Dialog_UI_Manager>();
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

        talkAction = playerInput.actions["Talk"];
        if (talkAction == null)
        {
            Debug.LogWarning("[Object_Dialogue_Interact] Could not find 'Talk' action in PlayerInput!");
            return;
        }

        talkAction.performed += ctx => PlayDialogInteraction();
        talkAction.Enable();

        Debug.Log("[Object_Dialogue_Interact] Input bound successfully.");
    }

    public void PlayDialogInteraction()
    {
        if (playerInsideTrigger && dialogUIManager.textTyping == false)
        {
            dialogUIManager.HidePortrait();
            dm.PlayScene(dialogKey);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInsideTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = false;
            dm.EndDialog();  // Safety call to ensure dialog box is closed
        }
    }
}
