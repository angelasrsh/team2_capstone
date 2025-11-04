using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring_Spawn_Manager : MonoBehaviour
{
    public Ingredient_Data ringData;
    public GameObject collectiblePrefab;
    public Transform spawnPoint;

    private bool spawnedThisSession = false;

    private void Start() => TrySpawnRing();
    private void TrySpawnRing()
    {
        if (spawnedThisSession) return;

        if (Affection_Reward_Tracker.instance == null)
        {
            Debug.LogWarning("[RingSpawnManager] No AffectionRewardTracker instance found.");
            return;
        }

        if (Affection_Reward_Tracker.instance.ringUnlocked && !Affection_Reward_Tracker.instance.ringCollected)
        {
            if (ringData == null || collectiblePrefab == null)
            {
                Debug.LogError("[RingSpawnManager] ringData or collectiblePrefab not assigned.");
                return;
            }

            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject obj = Instantiate(collectiblePrefab, pos, Quaternion.identity);

            var collectible = obj.GetComponent<Collectible_Object>();
            if (collectible != null)
            {
                collectible.Initialize(ringData);
                Debug.Log("[RingSpawnManager] Ring collectible instantiated and initialized.");
            }
            else
                Debug.LogWarning("[RingSpawnManager] Instantiated prefab has no Collectible_Object component. Are you using the correct prefab?");

            spawnedThisSession = true;
        }
    }
}
