using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Grimoire;
using UnityEngine;
using UnityEngine.SceneManagement;

// Script for an ingredient that can be picked up
public class CookingTool : InteractableObject
{
    // TODO: Make a new scriptable object for EquipmentTypes later if we want
    //[field: SerializeField] public ResourceInfo ResourceType { get; private set; }

    void Start()
    {
        // Set item sprite to the ResourceType
        //GetComponentInChildren<SpriteRenderer>().sprite = ResourceType.Image;
    }

    public override void PerformInteract(GameObject interactor)
    {
        Debug.Log($"[CookEqpt] {interactor} interacted on " + gameObject.ToString() + " performed");

        // If the interactor has an inventory, add yourself to it and disappear
        SceneManager.LoadScene("Cooking_Minigame");
    }
}
