using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[RequireComponent(typeof(SpriteRenderer), typeof(Collider))]  // should put this back in
public class Resource_Pickup : MonoBehaviour
{
    [field: SerializeField] public Ingredient_Data data { get; private set; }
    private bool playerInside = false;

    void Start() // Should maybe go in initialize, but haven't figured out when to initialize yet
    {
        gameObject.tag="Ingredient";
    }

    public void Initialize(Ingredient_Data newData)
    {

        data = newData;

        // var renderer = GetComponent<SpriteRenderer>(); Change this to affect child sprite object
        // if (renderer != null && data.Image != null)
        // {
        //     renderer.sprite = data.Image;
        // }

        gameObject.name = $"Pickup_{data.Name}";
    }

    private void Update() // May change this to OnTriggerStay
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            // Add checking for inventory status
            Debug.Log($"[Rscr_Pckp] Data is null? {data == null}");
            //Debug.Log($"Picked up {data.Name} (Tier {data.tier}, Price {data.price})");
            Destroy(gameObject);
        }
        Debug.Log($"Player inside? {playerInside}");
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log($"[Rsc_Pck] Player entered range of {this}");
        }
            
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}


/**
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Grimoire;
using UnityEngine;

// Script for an ingredient that can be picked up
public class Resource_Pickup : Interactable_Object
{
    [field: SerializeField] public Resource_Info ResourceType { get; private set; }

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
                Player_Controller scriptWithRange = interactor.GetComponent<Player_Controller>();
                if (scriptWithRange != null)
                {
                    // Play pickup sound
                    // GameObject.Find("Item_Pickup_SFX").GetComponent<Audio_Manager>().PlayOnce();
                    scriptWithRange.RemoveFromRange(this);
                }
                Destroy(gameObject);
            }

        }
    }
    
}
*/
