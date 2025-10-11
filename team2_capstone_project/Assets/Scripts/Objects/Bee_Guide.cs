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
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float buzzAmplitude = 0.05f;
    [SerializeField] private float buzzFrequency = 5f;

    [Header("Hover")]
    [SerializeField] private float hoverAmplitude = 0.02f;
    [SerializeField] private float hoverSpeed = 2f;   
    private float hoverBaseHeight;  // remembered height at start of hover

    private int currentWaypointIndex = 0;
    private bool isActive = false;
    private bool waitingForPlayer = false;

    public int CurrentWaypointIndex => currentWaypointIndex;
    public Transform CurrentWaypoint => waypoints[currentWaypointIndex];

    private Vector3 basePosition;
    private float buzzSeed;

    void Start()
    {
        basePosition = transform.position;
        buzzSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!isActive) return;

        if (waitingForPlayer)
        {
            IdleBuzz();
        }
        else
        {
            MoveTowardsWaypoint();
        }

        ApplyBuzzOffset();
    }

    private void MoveTowardsWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - basePosition).normalized;
        basePosition += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(basePosition, target.position) < stoppingDistance)
        {
            waitingForPlayer = true;
            basePosition = target.position;
        }
    }
    
    private void IdleBuzz()
    {
        // Only set once when bee starts waiting
        if (Mathf.Approximately(hoverBaseHeight, 0f))
            hoverBaseHeight = basePosition.y;

        // Calculate up/down offset
        float floatY = Mathf.Sin(Time.time * hoverSpeed + buzzSeed) * hoverAmplitude;
        basePosition.y = hoverBaseHeight + floatY;
    }

    private void ApplyBuzzOffset()
    {
        // Stable buzzing around base position
        float buzzX = Mathf.Sin(Time.time * buzzFrequency + buzzSeed) * buzzAmplitude;
        float buzzY = Mathf.Cos(Time.time * buzzFrequency + buzzSeed) * buzzAmplitude * 0.5f;

        transform.position = basePosition + new Vector3(buzzX, buzzY, 0);
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
