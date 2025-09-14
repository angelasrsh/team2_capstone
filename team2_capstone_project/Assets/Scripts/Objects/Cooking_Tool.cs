using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Grimoire;
using UnityEngine;
using UnityEngine.SceneManagement;

// Script for an ingredient that can be picked up
public class Cooking_Tool : Interactable_Object
{
    // TODO: Make a new scriptable object for EquipmentTypes later if we want
    //[field: SerializeField] public ResourceInfo ResourceType { get; private set; }

    void Start()
    {
        // Set item sprite to the ResourceType
        //GetComponentInChildren<SpriteRenderer>().sprite = ResourceType.Image;
    }

    public override void PerformInteract()
    {
        Debug.Log($"[CookEqpt] player interacted with " + gameObject.ToString());


        SceneManager.LoadScene("Cooking_Minigame");
    }
}
