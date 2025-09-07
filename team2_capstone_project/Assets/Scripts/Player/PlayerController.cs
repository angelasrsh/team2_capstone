using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Grimoire;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;

    // Movement
    public float moveSpeed = 5f;
    float horizontalMovement;
    float verticalMovement;

    // Interaction
    private List<InteractableObject> InteractableObjects;

    // Start called once
    void Start()
    {
        InteractableObjects = new List<InteractableObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // velocity is updated by Input system callbacks
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(horizontalMovement * moveSpeed, rb.velocity.y, verticalMovement * moveSpeed);

    }

    // Called by Unity Input system
    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        verticalMovement = context.ReadValue<Vector2>().y;
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Player calls OnTriggerEnter");


        // Keep a list of interactable objects within range
        // (i.e. this code won't work if the collider and script are on the same base object)

        // Sense colliders for interactable objects are children of the Interactable Object with the script
        GameObject parentObj = other.transform.parent.gameObject;

        if (parentObj.GetComponent<InteractableObject>() != null)
        {
            InteractableObject getObj = parentObj.gameObject.GetComponent<InteractableObject>();
            Debug.Log("[PC] Now in range: " + getObj);
            InteractableObjects.Add(getObj);
            Debug.Log("[PC] List size: " + InteractableObjects.Count);
        }

    }

    void OnTriggerExit(Collider other)
    {
        //Debug.Log("Player calls OnTriggerExit");


        // Update list of interactable objects by removing ones now out of range

        // Sense colliders for interactable objects are children of the Interactable Object with the script
        GameObject parentObj = other.transform.parent.gameObject;

        if (parentObj.GetComponent<InteractableObject>() != null)
        {
            InteractableObject getObj = parentObj.GetComponent<InteractableObject>();
            Debug.Log("[PC] Now out of range: " + getObj);
            InteractableObjects.Remove(getObj);
            Debug.Log("[PC] List size: " + InteractableObjects.Count);
            //Debug.Log("[PC] Interactable Objects:" + InteractableObjects);
        }
    }

    // Press 'E' to interact (called by Unity Input System)
    // Interacts on all objects in range in the order they entered-
    // may have some issues or undefined behavior with lots of objects together
    public void Interact(InputAction.CallbackContext context)
    {
        // Only interact on complete button presses
        if (!context.performed)
        {
            return;
        }

        Debug.Log("[PC] Interact called");
        Debug.Log("[PC] List size: " + InteractableObjects.Count);
        if (InteractableObjects == null)
        {
            return;
        }

        // Make a list to iterate through that doesn't change
        List<InteractableObject> tempSaveInteractableObjects = new List<InteractableObject>(InteractableObjects);

        foreach (InteractableObject obj in tempSaveInteractableObjects) // shallow copy- same references
        {
            Debug.Log("[PC] interact on " + obj.ToString() + ".");
            obj.PerformInteract(this.gameObject); // May remove item from original list
        }
    }

    // For when items go out of range and don't call OnTriggerExit();
    public void RemoveFromRange(InteractableObject obj)
    {
        Debug.Log($"[PC] told to remove {obj} from in-range list");
        InteractableObjects.Remove(obj);
    }
}
