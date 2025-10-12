using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grimoire;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Canvas_Inventory_Listener : MonoBehaviour
{
    private Canvas InventoryCanvas;
    private InputAction openInventory;

    private void Awake()
    {
        InventoryCanvas = GetComponent<Canvas>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        TryBindInput();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindInput(); // rebind safely after scene changes
    }

    private void TryBindInput()
    {
        if (Game_Manager.Instance == null)
        {
            Debug.LogWarning("[Canvas_Inventory_Listener] No Game_Manager found in scene.");
            return;
        }

        PlayerInput playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogWarning("[Canvas_Inventory_Listener] No PlayerInput found on Game_Manager.");
            return;
        }

        openInventory = playerInput.actions.FindAction("OpenInventory", true);
        if (openInventory == null)
        {
            Debug.LogWarning("[Canvas_Inventory_Listener] Could not find 'OpenInventory' action in PlayerInput.");
            return;
        }

        openInventory.Enable();

        Debug.Log("[Canvas_Inventory_Listener] Successfully bound to OpenInventory action.");
    }

    private void Update()
    {
        if (openInventory == null)
            return;

        if (openInventory.WasPerformedThisFrame())
        {
            if (InventoryCanvas == null)
            {
                Debug.LogWarning("[Canvas_Inventory_Listener] Error: no InventoryCanvas assigned!");
                return;
            }

            if (InventoryCanvas.enabled)
            {
                // Close
                InventoryCanvas.enabled = false;
                Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.bagClose, 1f);
            }
            else
            {
                // Open
                InventoryCanvas.enabled = true;
                Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.bagOpen, 0.9f);
            }

            Game_Events_Manager.Instance.InventoryToggled(InventoryCanvas.enabled);
        }
    }
}
