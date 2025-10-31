using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Foraging_NPC_Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<CustomerData> possibleNPCs = new List<CustomerData>();
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private Foraging_Area_NPC_Actor npcPrefab;
    [SerializeField] private int maxNPCs = 3;

    [Range(0f, 1f)]
    [Tooltip("Overall chance that an NPC will spawn at each spawn point.")]
    [SerializeField] private float baseSpawnChance = 0.5f;

    [Header("Affection Requirements")]
    [SerializeField] private int affectionThreshold = 25;
    [SerializeField] private int guaranteedSpawnAffection = 75;
    [SerializeField] private bool requireUnlockedNPC = true;

    private List<Foraging_Area_NPC_Actor> activeNPCs = new List<Foraging_Area_NPC_Actor>();
    private HashSet<string> activeNPCIDs = new HashSet<string>();

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SpawnEligibleNPCs();
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnDestroy() => ClearExistingNPCs();

    public void SpawnEligibleNPCs()
    {
        ClearExistingNPCs();

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points assigned for Foraging_NPC_Spawner!");
            return;
        }

        var affectionSys = Affection_System.Instance;
        var playerProg = Player_Progress.Instance;
        List<CustomerData> eligibleNPCs = new List<CustomerData>();

        // --- 1. Gather eligible NPCs ---
        foreach (var npc in possibleNPCs)
        {
            if (npc == null)
                continue;

            if (requireUnlockedNPC && !playerProg.IsNPCUnlocked(npc.npcID))
                continue;

            int affection = affectionSys.GetAffectionLevel(npc);
            if (affection < affectionThreshold)
                continue;

            eligibleNPCs.Add(npc);
        }

        if (eligibleNPCs.Count == 0)
        {
            Debug.Log("No eligible NPCs to spawn based on affection/unlocks.");
            return;
        }

        // --- 2. Separate guaranteed spawns ---
        List<CustomerData> guaranteedSpawns = new List<CustomerData>();
        List<CustomerData> weightedPool = new List<CustomerData>();

        foreach (var npc in eligibleNPCs)
        {
            int affection = affectionSys.GetAffectionLevel(npc);

            if (affection >= guaranteedSpawnAffection)
                guaranteedSpawns.Add(npc);
            else
                weightedPool.Add(npc);
        }

        // --- 3. Spawn guaranteed NPCs first ---
        foreach (var npc in guaranteedSpawns)
        {
            if (activeNPCs.Count >= maxNPCs)
                break;
            if (activeNPCIDs.Contains(npc.npcID.ToString()))
                continue;

            Transform point = GetFreeSpawnPoint();
            if (point == null)
                break;

            SpawnNPC(npc, point);
        }

        // --- 4. Spawn remaining NPCs using affection weighting ---
        while (activeNPCs.Count < maxNPCs)
        {
            Transform point = GetFreeSpawnPoint();
            if (point == null)
                break;

            if (Random.value > baseSpawnChance)
                continue;  

            // Build weighted random selection
            CustomerData chosen = WeightedRandomNPC(weightedPool, affectionSys);
            if (chosen == null)
                break;

            // Prevent duplicates
            if (activeNPCIDs.Contains(chosen.npcID.ToString()))
                continue;

            SpawnNPC(chosen, point);
        }
    }

    private CustomerData WeightedRandomNPC(List<CustomerData> npcs, Affection_System affSys)
    {
        if (npcs == null || npcs.Count == 0)
            return null;

        float totalWeight = 0f;
        Dictionary<CustomerData, float> weights = new Dictionary<CustomerData, float>();

        foreach (var npc in npcs)
        {
            int affection = affSys.GetAffectionLevel(npc);
            float weight = Mathf.Max(1, affection); // affection directly = weight
            weights[npc] = weight;
            totalWeight += weight;
        }

        float roll = Random.Range(0, totalWeight);
        float cumulative = 0f;

        foreach (var kvp in weights)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
                return kvp.Key;
        }

        return npcs[Random.Range(0, npcs.Count)];
    }

    private void SpawnNPC(CustomerData npc, Transform point)
    {
        if (npc == null || point == null)
            return;

        Foraging_Area_NPC_Actor npcInstance = Instantiate(npcPrefab, point.position, Quaternion.identity);
        npcInstance.Init(npc, point);
        activeNPCs.Add(npcInstance);
        activeNPCIDs.Add(npc.npcID.ToString());

        // cleanup tracker
        var tracker = npcInstance.gameObject.AddComponent<Foraging_NPC_Tracker>();
        tracker.Init(this, npc.npcID.ToString());

        Debug.Log($"Spawned foraging NPC: {npc.customerName} at {point.name}");
    }

    private Transform GetFreeSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
            return null;

        // Make a shuffled list of all spawn points
        List<Transform> shuffledPoints = new List<Transform>(spawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            int randIndex = Random.Range(i, shuffledPoints.Count);
            (shuffledPoints[i], shuffledPoints[randIndex]) = (shuffledPoints[randIndex], shuffledPoints[i]);
        }

        // Return the first unoccupied one
        foreach (Transform t in shuffledPoints)
        {
            bool occupied = false;
            foreach (var npc in activeNPCs)
            {
                if (npc != null && Vector3.Distance(npc.transform.position, t.position) < 0.5f)
                {
                    occupied = true;
                    break;
                }
            }
            if (!occupied)
                return t;
        }

        return null;
    }

    public void UnregisterNPC(string npcID)
    {
        if (activeNPCIDs.Contains(npcID))
            activeNPCIDs.Remove(npcID);
    }

    private void ClearExistingNPCs()
    {
        foreach (var npc in activeNPCs)
        {
            if (npc != null)
                Destroy(npc.gameObject);
        }

        activeNPCs.Clear();
        activeNPCIDs.Clear();
    }
}
