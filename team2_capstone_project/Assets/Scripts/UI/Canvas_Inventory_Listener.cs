using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grimoire;
using UnityEngine.InputSystem;

public class Canvas_Inventory_Listener : MonoBehaviour
{
    private Canvas InventoryCanvas;
    private InputAction openInventory;

    // Set gameObject to the canvas this is on
    void Start()
    {
        InventoryCanvas = this.gameObject.GetComponent<Canvas>();

        Player_Input_Controller pic = FindObjectOfType<Player_Input_Controller>();
        if (pic != null)
        {
            openInventory = pic.GetComponent<PlayerInput>().actions["OpenInventory"];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (openInventory.WasPerformedThisFrame())
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
