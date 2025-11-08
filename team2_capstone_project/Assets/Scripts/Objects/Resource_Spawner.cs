using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Grimoire
{
    [System.Serializable]
    public struct SpawnArea
    {
        public Vector3 centerOffset;
        public Vector3 size;

        [Header("Spawn Limits")]
        public int minSpawn;
        public int maxSpawn;

        [Header("Resources in This Area")]
        public Ingredient_Data[] possibleResources;
    }

    public class Resource_Spawner : MonoBehaviour
    {
        [Header("Global Spawn Limits")]
        public int globalMinSpawn = 15;
        public int globalMaxSpawn = 30;

        [Header("Spawner Settings")]
        public GameObject interactablePrefab;
        public int totalToSpawn = 10;

        [Header("Spawn Areas")]
        public List<SpawnArea> spawnAreas = new List<SpawnArea>();

        private void Start()
        {
            SpawnResources();
        }

        /// <summary>
        /// Spawns resources based on weighted probabilities, ensuring no overlap and respecting spawn area limits.
        /// </summary>
        private void SpawnResources()
        {
            if (spawnAreas.Count == 0)
                return;

            // Step 1: Determine total global spawn count
            int totalGlobal = Random.Range(globalMinSpawn, globalMaxSpawn + 1);

            // Step 2: Randomize how many to spawn per area within its own limits
            List<int> zoneSpawnCounts = new List<int>();
            int totalRequested = 0;

            foreach (var area in spawnAreas)
            {
                int count = Random.Range(area.minSpawn, area.maxSpawn + 1);
                zoneSpawnCounts.Add(count);
                totalRequested += count;
            }

            // Normalize if total exceeds global limit
            if (totalRequested > totalGlobal)
            {
                float scale = totalGlobal / (float)totalRequested;
                for (int i = 0; i < zoneSpawnCounts.Count; i++)
                    zoneSpawnCounts[i] = Mathf.RoundToInt(zoneSpawnCounts[i] * scale);
            }

            // Step 3: Spawn for each area independently
            List<Vector3> usedPositions = new List<Vector3>();
            float minDistanceBetweenSpawns = 1.5f;

            for (int z = 0; z < spawnAreas.Count; z++)
            {
                SpawnArea area = spawnAreas[z];
                int toSpawn = zoneSpawnCounts[z];

                // Create a local spawn pool for this area
                List<Ingredient_Data> localPool = new List<Ingredient_Data>();

                foreach (var resource in area.possibleResources)
                {
                    // Assuming Ingredient_Data has these fields:
                    // int minSpawn, int maxSpawn, float rarityWeight
                    int amount = Random.Range(1, 4); // Or use minSpawn/maxSpawn if defined on resource
                    int weight = Mathf.RoundToInt(resource.rarityWeight * 100);

                    for (int i = 0; i < amount; i++)
                        for (int w = 0; w < weight; w++)
                            localPool.Add(resource);
                }

                // Skip area if no resources configured
                if (localPool.Count == 0)
                    continue;

                // Step 4: Actually spawn within this area
                for (int i = 0; i < toSpawn; i++)
                {
                    Ingredient_Data chosen = localPool[Random.Range(0, localPool.Count)];
                    Vector3? pos = GetSafeSpawnPosition(area, usedPositions, minDistanceBetweenSpawns);

                    if (pos.HasValue)
                    {
                        usedPositions.Add(pos.Value);
                        GameObject obj = Instantiate(interactablePrefab, pos.Value, Quaternion.identity);
                        var pickup = obj.GetComponent<Collectible_Object>() ?? obj.AddComponent<Collectible_Object>();
                        pickup.Initialize(chosen);
                    }
                }
            }
        }


        /// <summary>
        /// Attempts to find a safe spawn position within the given area that does not overlap with existing positions or colliders.
        /// </summary>
        private Vector3? GetSafeSpawnPosition(SpawnArea area, List<Vector3> usedPositions, float bufferRadius, int maxAttempts = 10, float colliderCheckRadius = 0.5f)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 candidate = transform.position + area.centerOffset + new Vector3(
                    Random.Range(-area.size.x / 2, area.size.x / 2),
                    0,
                    Random.Range(-area.size.z / 2, area.size.z / 2)
                );

                // 1. Avoid colliders (trees, rocks, etc.)
                Collider[] hits = Physics.OverlapSphere(candidate, colliderCheckRadius);
                bool blocked = false;
                foreach (var hit in hits)
                {
                    if (!hit.isTrigger)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) continue;

                // 2. Avoid overlap with other resources
                bool tooClose = false;
                foreach (var pos in usedPositions)
                {
                    if (Vector3.Distance(candidate, pos) < bufferRadius)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                // Passed both checks
                return candidate;
            }
            return null; // No valid position found
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
