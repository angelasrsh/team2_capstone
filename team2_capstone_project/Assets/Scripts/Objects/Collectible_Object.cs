using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;


//[RequireComponent(typeof(SpriteRenderer), typeof(Collider))]  // should put this back in
public class Collectible_Object : Interactable_Object
{
    [field: SerializeField] public Ingredient_Data data { get; private set; }



    /// <summary>
    /// Am not yet sure where this should come into play
    /// </summary>
    /// <param name="newData"></param>
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

    public override void PerformInteract()
    {
        Debug.Log($"[Col_Obj] player interacted with " + gameObject.ToString());

        // Add to inventory
        Ingredient_Inventory.Instance.AddResources(data, 1);
        Destroy(gameObject);
    }
}