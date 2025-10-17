using System.Collections.Generic;
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
    }

    public class Resource_Spawner : MonoBehaviour
    {
        [Header("Global Spawn Limits")]
        public int globalMinSpawn = 15;
        public int globalMaxSpawn = 30;

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

        /// <summary>
        /// Spawns resources based on weighted probabilities, ensuring no overlap and respecting spawn area limits.
        /// </summary>
        private void SpawnResources()
        {
            // Step 1: Weighted pool creation (unchanged)
            List<Ingredient_Data> spawnPool = new List<Ingredient_Data>();

            foreach (var resource in possibleResources)
            {
                int amount = Random.Range(resource.minSpawn, resource.maxSpawn + 1);
                int weight = Mathf.RoundToInt(resource.rarityWeight * 100);

                for (int i = 0; i < amount; i++)
                {
                    for (int w = 0; w < weight; w++)
                        spawnPool.Add(resource);
                }
            }

            if (spawnPool.Count == 0 || spawnAreas.Count == 0)
                return;

            // Step 2: Determine total to spawn (global limits)
            int totalGlobal = Random.Range(globalMinSpawn, globalMaxSpawn + 1);

            // Step 3: Distribute spawns across zones proportionally
            int remaining = totalGlobal;
            List<int> zoneSpawnCounts = new List<int>();

            // First pass: Assign random count per zone within its min/max
            foreach (var area in spawnAreas)
            {
                int count = Random.Range(area.minSpawn, area.maxSpawn + 1);
                zoneSpawnCounts.Add(count);
            }

            // Normalize so total doesn’t exceed global limit
            int sum = 0;
            foreach (int c in zoneSpawnCounts) sum += c;

            if (sum > totalGlobal)
            {
                float scale = totalGlobal / (float)sum;
                for (int i = 0; i < zoneSpawnCounts.Count; i++)
                    zoneSpawnCounts[i] = Mathf.RoundToInt(zoneSpawnCounts[i] * scale);
            }

            // Step 4: Spawn per zone with overlap + collision checks
            List<Vector3> usedPositions = new List<Vector3>();
            float minDistanceBetweenSpawns = 1.5f; // buffer between resources

            for (int z = 0; z < spawnAreas.Count; z++)
            {
                SpawnArea area = spawnAreas[z];
                int toSpawn = zoneSpawnCounts[z];

                for (int i = 0; i < toSpawn; i++)
                {
                    if (spawnPool.Count == 0) return;

                    Ingredient_Data chosen = spawnPool[Random.Range(0, spawnPool.Count)];

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
