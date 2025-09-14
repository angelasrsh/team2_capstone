using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider))]
    public class Resource_Pickup : MonoBehaviour
    {
        public Resource_Data data { get; private set; }
        private bool playerInside = false;

        public void Initialize(Resource_Data newData)
        {
            data = newData;

            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && data.resourceSprite != null)
            {
                renderer.sprite = data.resourceSprite;
            }

            gameObject.name = $"Pickup_{data.resourceName}";
        }

        private void Update()
        {
            if (playerInside && Input.GetKeyDown(KeyCode.E))
            {
                // Add checking for inventory status
                Debug.Log($"Picked up {data.resourceName} (Tier {data.tier}, Price {data.price})");
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                playerInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                playerInside = false;
        }
    }
}
