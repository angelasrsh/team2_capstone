using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring_Spawn_Manager : MonoBehaviour
{
    public CustomerData targetNPC;
    public GameObject collectiblePrefab;
    public Transform spawnPoint;

    private bool spawnedThisSession = false;

    private void Start() => TrySpawnEventItem();

    private void TrySpawnEventItem()
    {
        if (spawnedThisSession) return;

        var tracker = Affection_Event_Item_Tracker.instance;
        if (tracker == null)
        {
            Debug.LogWarning("[Ring_Spawn_Manager] No AffectionEventItemTracker instance found.");
            return;
        }

        Debug.Log($"[Ring_Spawn_Manager] Searching for NPC ID: {targetNPC.npcID}");
        foreach (var i in tracker.items)
            Debug.Log($"[Ring_SpawnManager] Tracker entry: {i.npc?.customerName} (ID: {i.npc?.npcID}), unlocked={i.unlocked}, collected={i.collected}");


        // Check for an unlocked event item for the target NPC
        if (!tracker.TryGetUnlockedEventItem(targetNPC, out var specialItem))
        {
            Debug.Log("[Ring_Spawn_Manager] No unlocked event item found for target NPC.");
            return;
        }

        if (specialItem.collected)
        {
            Debug.Log("[Ring_Spawn_Manager] Special item already collected — disabling spawner.");
            DisableSpawner();
            return;
        }

        if (specialItem.eventItem == null || collectiblePrefab == null)
        {
            Debug.LogError("[RingSpawnManager] rewardItem or collectiblePrefab not assigned.");
            return;
        }

        // Spawn the item
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        GameObject obj = Instantiate(collectiblePrefab, pos, Quaternion.identity);

        var collectible = obj.GetComponent<Collectible_Object>();
        if (collectible != null)
        {
            collectible.Initialize(specialItem.eventItem);
            Debug.Log($"[Ring_Spawn_Manager] Spawned collectible for {specialItem.eventItem.Name}.");

            // Disable spawner after spawning
            DisableSpawner();
        }
        else
            Debug.LogWarning("[RingSpawnManager] prefab missing Collectible_Object component.");

        spawnedThisSession = true;
    }

    private void DisableSpawner()
    {
        // Prevents future calls
        spawnedThisSession = true;
        this.enabled = false;
    }
}
