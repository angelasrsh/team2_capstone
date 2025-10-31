using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipe_Spawn_Manager : MonoBehaviour
{
    public static Recipe_Spawn_Manager Instance;

    private List<Transform> allSpawnPoints = new List<Transform>();
    private HashSet<Transform> occupiedPoints = new HashSet<Transform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Gather all child transforms as spawn points
        foreach (Transform child in transform)
        {
            allSpawnPoints.Add(child);
        }

        Debug.Log($"[Recipe_Spawn_Manager] Registered {allSpawnPoints.Count} spawn points.");
    }

    public Transform GetRandomAvailablePoint()
    {
        List<Transform> freePoints = allSpawnPoints.FindAll(p => !occupiedPoints.Contains(p));
        if (freePoints.Count == 0)
        {
            Debug.LogWarning("[Recipe_Spawn_Manager] No free spawn points!");
            return null;
        }

        Transform chosen = freePoints[Random.Range(0, freePoints.Count)];
        occupiedPoints.Add(chosen);
        return chosen;
    }

    public void FreePoint(Transform point)
    {
        if (occupiedPoints.Contains(point))
            occupiedPoints.Remove(point);
    }
}
