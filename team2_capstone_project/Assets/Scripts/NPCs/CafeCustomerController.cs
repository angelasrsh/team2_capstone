using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CafeCustomerController : MonoBehaviour
{
    public CustomerData data;
    
    [SerializeField] private GameObject thoughtBubble;
    [SerializeField] private Image bubbleDishImage;

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
        Debug.Log($"Customer spawned at {transform.position}, isOnNavMesh = {agent.isOnNavMesh}");
    }

    void Update()
    {
        if (seat != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SitDown();
        }
    }

    void LateUpdate()
    {
        transform.forward = Vector3.forward;
    }

    private void SitDown()
    {
        agent.isStopped = true;

        // Pick a random favorite dish
        if (data.favoriteDishes.Length > 0)
        {
            DishData chosenDish = data.favoriteDishes[Random.Range(0, data.favoriteDishes.Length)];

            // Enable bubble & set sprite dynamically
            thoughtBubble.SetActive(true);
            bubbleDishImage.sprite = chosenDish.dishSprite;

            Debug.Log($"{data.customerName} wants {chosenDish.dishName}!");
        }

        seat = null; // Prevent repeating
    }
}