using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
    public class Environment_Forgeable : Interactable_Object
    {
        [Header("Forage Settings")]
        [SerializeField] private Ingredient_Data ingredientData;
        [SerializeField] private Forageable_Data forageableData;
        [SerializeField] private GameObject foragedVariant; 
        [SerializeField] private bool destroyOnForage = false;  

        private bool isForaged = false;

        public override void PerformInteract()
        {
            if (isForaged) return;

            Debug.Log($"[Forageable] Player foraged {ingredientData.Name}");

            // Check if we should add a specific number of resources (default is 1 otherwise)
            int amountToAdd = 1;
            if (forageableData != null)
                amountToAdd = forageableData.ResourcesToGive;

            Ingredient_Inventory.Instance.AddResources(ingredientData, amountToAdd);
            Audio_Manager.instance.PlaySFX(Audio_Manager.instance.pickupSFX);
            isForaged = true;

            if (foragedVariant != null)
            {
                // Replace with foraged object
                Instantiate(foragedVariant, transform.position, transform.rotation);
                Destroy(gameObject);
            }
            else if (destroyOnForage)
            {
                Destroy(gameObject);
            }
            else
            {
                // Default fallback: disable collider & change appearance
                var collider = GetComponent<Collider>();
                if (collider != null) collider.enabled = false;

                var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null) spriteRenderer.color = Color.gray;
            }

            // Broadcast message to listening scripts
            Game_Events_Manager.Instance.Harvest(); // Currently unused until foraging is implemented
        }
    }
}
