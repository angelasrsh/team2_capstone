using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CafeCustomerController : MonoBehaviour
{
    [Header("Assigned Data")]
    public CustomerData data;

    [Header("UI")]
    public GameObject thoughtBubblePrefab;

    // Internal components
    private NavMeshAgent agent;
    private Transform seat;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Init(CustomerData customerData, Transform targetSeat)
    {
        data = customerData;
        seat = targetSeat;
        agent.SetDestination(seat.position);
    }

    void Update()
    {
        if (seat != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SitDown();
        }
    }

    private void SitDown()
    {
        agent.isStopped = true;

        Vector3 bubblePos = transform.position + Vector3.up * 2f;
        GameObject bubble = Instantiate(thoughtBubblePrefab, bubblePos, Quaternion.identity, transform);

        // Pick a random dish from favorites (will later be changed to from menu set night before)
        if (data.favoriteDishes.Length > 0)
        {
            string chosenDish = data.favoriteDishes[Random.Range(0, data.favoriteDishes.Length)];
            Debug.Log($"{data.customerName} wants {chosenDish}!");
        }

        seat = null; // Prevent repeating
    }
}