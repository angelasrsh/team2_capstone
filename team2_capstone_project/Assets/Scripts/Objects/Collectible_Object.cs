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

        if (spriteRenderer != null && data != null && data.Image != null)
            spriteRenderer.sprite = data.Image;

        gameObject.name = $"Pickup_{data.Name}";
    }

    public override void PerformInteract()
    {
        Debug.Log($"[Collectible_Object] Player picked up {data?.Name ?? "unknown"}");

        Audio_Manager.instance.PlaySFX(Audio_Manager.instance.bagPutIn, 0.4f, Random.Range(0.9f, 1.1f));
        Audio_Manager.instance.PlaySparkleSFX();
        Audio_Manager.instance.PlaySFX(Audio_Manager.instance.pickupSFX, 0.6f, 1f);

        Ingredient_Inventory.Instance.AddResources(data, 1);
        Destroy(gameObject);
    }
}
