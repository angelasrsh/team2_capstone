using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Lamp_On_Off : MonoBehaviour
{
    private Light lampLight;
    private InputAction interactAction;
    private bool playerInsideTrigger;
    
    private void Start()
    {
        lampLight = GetComponent<Light>();
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

        interactAction.performed += ctx => LampSwitch();
        interactAction.Enable();

        Debug.Log("[Object_Dialogue_Interact] Input bound successfully.");
    }

    public void LampSwitch()
    {
        if (playerInsideTrigger)
        {
            Audio_Manager.instance.PlaySFX(Audio_Manager.instance.lampSwitch, 0.7f, Random.Range(0.95f, 1.05f));
            lampLight.enabled = !lampLight.enabled;
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
            playerInsideTrigger = false;
    }
}
