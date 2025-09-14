using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
    public class ResourceSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        public ResourceData[] possibleResources;
        public GameObject interactablePrefab;
        public int totalToSpawn = 10;
        public Vector3 spawnAreaSize = new Vector3(10, 0, 10);

        private void Start()
        {
            SpawnResources();
        }

        private void SpawnResources()
        {
            // Step 1: Create weighted spawn pool
            List<ResourceData> spawnPool = new List<ResourceData>();

            foreach (var resource in possibleResources)
            {
                int amount = Random.Range(resource.minSpawn, resource.maxSpawn + 1);

                for (int i = 0; i < amount; i++)
                {
                    // Add to pool multiple times depending on weight
                    int weight = Mathf.RoundToInt(resource.rarityWeight * 100);
                    for (int w = 0; w < weight; w++)
                    {
                        spawnPool.Add(resource);
                    }
                }
            }

            // Step 2: Randomly spawn from pool
            for (int i = 0; i < totalToSpawn; i++)
            {
                if (spawnPool.Count == 0) break;

                // Pick a random resource from pool
                int index = Random.Range(0, spawnPool.Count);
                ResourceData chosen = spawnPool[index];

                // Remove to not overspawn
                spawnPool.RemoveAt(index);

                // Spawn prefab in random position within area
                Vector3 spawnPos = transform.position + new Vector3(
                    Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                    0,
                    Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
                );

               GameObject obj = Instantiate(interactablePrefab, spawnPos, Quaternion.identity);

                var pickup = obj.GetComponent<ResourcePickup>();
                if (pickup == null)
                {
                    pickup = obj.AddComponent<ResourcePickup>();
                }
                pickup.Initialize(chosen);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, spawnAreaSize);
        }
    }
}
