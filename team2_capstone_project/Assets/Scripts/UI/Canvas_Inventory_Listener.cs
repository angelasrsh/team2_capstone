using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grimoire;
using UnityEngine.InputSystem;

public class Canvas_Inventory_Listener : MonoBehaviour
{
  private Canvas InventoryCanvas;
  private InputAction openInventory; // open inventory action from player map
  private InputAction openInventoryUI; // open inventory action from ui map
  private PlayerInput playerInput;

  // Set gameObject to the canvas this is on
  void Start()
  {
    InventoryCanvas = this.gameObject.GetComponent<Canvas>();
    playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

    if (playerInput != null)
    {
      openInventory = playerInput.actions.FindActionMap("Player").FindAction("OpenInventory"); // From Player Input Map
      openInventoryUI = playerInput.actions.FindActionMap("UI").FindAction("OpenInventory"); // From UI Input Map
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (openInventory.WasPerformedThisFrame() || openInventoryUI.WasPerformedThisFrame())
    {
      if (InventoryCanvas == null)
        Debug.LogWarning("[Canv_Inv_Lis] Error: no InventoryCanvas assigned!");

      else if (InventoryCanvas.enabled == true)
      {
        // If open, close the inventory
        InventoryCanvas.enabled = false;
        Audio_Manager.instance.PlaySFX(Audio_Manager.instance.bagClose, 0.28f);
        Game_Events_Manager.Instance.InventoryToggled(InventoryCanvas.enabled);
      }
      else
      {
        InventoryCanvas.enabled = true; // If closed, open the inventory
        Audio_Manager.instance.PlaySFX(Audio_Manager.instance.bagOpen, 0.28f);
        Game_Events_Manager.Instance.InventoryToggled(InventoryCanvas.enabled);
      }       
    }
  }
}
