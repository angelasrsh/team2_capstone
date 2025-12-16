using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;

public class Collectible_Object : Interactable_Object
{
    [field: SerializeField] public Ingredient_Data data { get; private set; }
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject pickupPrefab;

    public void Initialize(Ingredient_Data newData)
    {
        data = newData;

        if (spriteRenderer != null && data != null && data.Image != null)
            spriteRenderer.sprite = data.Image;

        gameObject.name = $"Harvestable_{data.Name}";
    }

    public override void PerformInteract()
    {
        Debug.Log($"[Collectible_Object] Harvested {data?.Name ?? "unknown"}");

        Audio_Manager.instance.PlaySFX(Audio_Manager.instance.bagPutIn, 0.4f, Random.Range(0.9f, 1.1f));
        Audio_Manager.instance.PlaySparkleSFX();

        // spawn pickup away on XZ plane
        if (pickupPrefab != null && player != null)
        {
            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;

            Vector3 away;
            if (toPlayer.sqrMagnitude < 0.01f)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                away = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            }
            else
            {
                away = -toPlayer.normalized;
                away = Quaternion.Euler(0f, Random.Range(-40f, 40f), 0f) * away;
            }

            float distance = Random.Range(2.0f, 3.2f); // bigger distance so it's visible
            Vector3 spawnPos = transform.position + away * distance;
            spawnPos.y = transform.position.y + 0.25f;

            var pickupObj = Instantiate(pickupPrefab, spawnPos, Quaternion.identity);
            var pickup = pickupObj.GetComponent<Pickup_Item>();
            if (pickup != null)
                pickup.Initialize(data);

            // Mark as collected in affection event tracker if applicable
            if (Affection_Event_Item_Tracker.instance != null && data != null)
            {
                var entry = Affection_Event_Item_Tracker.instance.items.Find(r => r.eventItem == data);
                if (entry != null && !entry.collected)
                {
                    Affection_Event_Item_Tracker.instance.MarkCollected(entry);
                    Debug.Log($"[Collectible_Object] Marked {data.Name} as collected in event tracker.");
                }
            }
        }

        // --- Hide interact icon ---
        playerInside = false;
        if (InteractIcon != null)
        {
            // prefer destroy if it's a child instance
            if (InteractIcon.transform.IsChildOf(transform))
            {
                Destroy(InteractIcon);
                Debug.Log("[Collectible_Object] InteractIcon destroyed (child).");
            }
            else
            {
                InteractIcon.SetActive(false);
                Debug.Log("[Collectible_Object] InteractIcon set inactive (not child).");
            }
        }

        // --- Hide visuals and collider ---
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        Collider col = GetComponent<Collider>();
        CapsuleCollider capsuleCol = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = false;
        if (capsuleCol != null) capsuleCol.enabled = false;

        if (data.ingredientType == IngredientType.Uncut_Ficklegourd)
        {
            // --- If any audio is playing, pause it ---
            AudioSource audio = GetComponent<AudioSource>();
            audio?.Pause();

            gameObject.GetComponent<Timed_Collectible>().isCollected = true;
        }

        enabled = false;

    }
}
