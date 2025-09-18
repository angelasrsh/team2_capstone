using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas_Inventory_Listener : MonoBehaviour
{
    private Canvas InventoryCanvas;

    // Set gameObject to the canvas this is on
    void Start()
    {
        InventoryCanvas = this.gameObject.GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (InventoryCanvas == null)
                Debug.LogWarning("[Canv_Inv_Lis] Error: no InventoryCanvas assigned!");
                
            else if (InventoryCanvas.enabled == true) // If open, close the inventory
                InventoryCanvas.enabled = false;
            else
                InventoryCanvas.enabled = true; // If closed, open the inventory
                
        }
    }
}
