using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee_Checkpoint : MonoBehaviour
{
    [SerializeField] private Bee_Guide bee;
    [SerializeField] private int waypointIndex; // 0-based index matching BeeGuide's waypoint list

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Compare the bee's current target index
            if (bee.CurrentWaypointIndex == waypointIndex)
            {
                bee.PlayerReachedCheckpoint();
            }
        }
    }
}
