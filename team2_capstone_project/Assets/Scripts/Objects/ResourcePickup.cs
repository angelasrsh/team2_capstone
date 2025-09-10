using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Grimoire;
using UnityEngine;

// Script for an ingredient that can be picked up
public class ResourcePickup : InteractableObject
{
    [field: SerializeField] public ResourceInfo ResourceType { get; private set; }

    void Start()
    {
        // Set item sprite to the ResourceType
        GetComponentInChildren<SpriteRenderer>().sprite = ResourceType.Image;
    }

    public override void PerformInteract(GameObject interactor)
    {
        Debug.Log($"[RsrPkcp] {interactor} interacted on " + gameObject.ToString() + " performed");

        // If the interactor has an inventory, add yourself to it and disappear
        Inventory inventoryToJoin = interactor.GetComponent<Inventory>();
        if (inventoryToJoin != null)
        {
            int added = inventoryToJoin.AddResources(ResourceType, 1); // Add self to inventory
            // Only disappear if item was actually added
            if (added > 0)
            {
              
                // If the interactor is player with a PlayerController, remove yourself
                PlayerController scriptWithRange = interactor.GetComponent<PlayerController>();
                if (scriptWithRange != null)
                {
                    // Play pickup sound
                    GameObject.Find("Item_Pickup_SFX").GetComponent<Audio_Manager>().PlayOnce();
                    scriptWithRange.RemoveFromRange(this);
                }
                Destroy(gameObject);
            }

        }
    }
}
