using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee_Guide : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float stoppingDistance = 0.2f;
    [SerializeField] private GameObject resourceToReveal;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float buzzAmplitude = 0.1f;
    [SerializeField] private float buzzFrequency = 5f;

    private Vector3 basePosition;
    private int currentWaypointIndex = 0;
    private bool isActive = false;
    private bool waitingForPlayer = false;

    public int CurrentWaypointIndex => currentWaypointIndex;
    public Transform CurrentWaypoint => waypoints[currentWaypointIndex];

    void Start()
    {
        basePosition = transform.position;
    }

    void Update()
    {
        if (!isActive || waitingForPlayer) return;
        MoveTowardsWaypoint();
    }

   private void MoveTowardsWaypoint()
   {

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Add bee-like buzzing motion
        float noiseX = Mathf.PerlinNoise(Time.time * buzzFrequency, 0f) - 0.5f;
        float noiseY = Mathf.PerlinNoise(0f, Time.time * buzzFrequency) - 0.5f;
        Vector3 buzzOffset = new Vector3(noiseX, noiseY, 0f) * buzzAmplitude;
        transform.position += buzzOffset;

        if (Vector3.Distance(transform.position, target.position) < stoppingDistance)
            waitingForPlayer = true;
    }

    public void PlayerReachedCheckpoint()
    {
        if (!isActive) return;

        waitingForPlayer = false;
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Length)
        {
            OnPathComplete();
        }
    }

    private void OnPathComplete()
    {
        isActive = false;

        if (resourceToReveal != null)
        {
            resourceToReveal.SetActive(true);
            Debug.Log("[BeeGuide] Resource activated and ready to forage!");
        }
    }


    public void Activate()
    {
        if (!isActive)
        {
            isActive = true;
            currentWaypointIndex = 0;
        }
    }

    public Transform GetWaypointAtIndex(int index)
    {
        if (index < 0 || index >= waypoints.Length) return null;
        return waypoints[index];
    }
}
