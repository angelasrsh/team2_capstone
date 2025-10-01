using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
public class Collectible_Object : Interactable_Object
{
    [field: SerializeField] public Ingredient_Data data { get; private set; }
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Initialize(Ingredient_Data newData)
    {
        data = newData;

        // Update sprite if both are set
        if (spriteRenderer != null && data != null && data.Image != null)
        {
            spriteRenderer.sprite = data.Image;
        }

        gameObject.name = $"Pickup_{data.Name}";
    }

    public override void PerformInteract()
    {
        Debug.Log($"[Col_Obj] player interacted with " + gameObject);

        Audio_Manager.instance.PlaySFX(Audio_Manager.instance.pickupSFX);
        Audio_Manager.instance.SetSFXVolume(0.5f);

        Ingredient_Inventory.Instance.AddResources(data, 1);
        Destroy(gameObject);
    }
}
