using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
    [System.Serializable]
    public struct SpawnArea
    {
        public Vector3 centerOffset; 
        public Vector3 size;        
    }

    public class Resource_Spawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        public Ingredient_Data[] possibleResources;
        public GameObject interactablePrefab;
        public int totalToSpawn = 10;

        [Header("Spawn Areas")]
        public List<SpawnArea> spawnAreas = new List<SpawnArea>();

        private void Start()
        {
            SpawnResources();
        }

        private void SpawnResources()
        {
            // Step 1: Create weighted spawn pool
            List<Ingredient_Data> spawnPool = new List<Ingredient_Data>();

            foreach (var resource in possibleResources)
            {
                int amount = Random.Range(resource.minSpawn, resource.maxSpawn + 1);

                for (int i = 0; i < amount; i++)
                {
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

                // Pick a random resource
                int index = Random.Range(0, spawnPool.Count);
                Ingredient_Data chosen = spawnPool[index];
                spawnPool.RemoveAt(index);

                // Pick a random spawn area
                SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Count)];

                // Pick a random position inside that area
                Vector3 spawnPos = transform.position + area.centerOffset + new Vector3(
                    Random.Range(-area.size.x / 2, area.size.x / 2),
                    0,
                    Random.Range(-area.size.z / 2, area.size.z / 2)
                );

                // Spawn
                GameObject obj = Instantiate(interactablePrefab, spawnPos, Quaternion.identity);
                var pickup = obj.GetComponent<Collectible_Object>() ?? obj.AddComponent<Collectible_Object>();
                pickup.Initialize(chosen);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            foreach (var area in spawnAreas)
            {
                Gizmos.DrawWireCube(transform.position + area.centerOffset, area.size);
            }
        }
    }
}
