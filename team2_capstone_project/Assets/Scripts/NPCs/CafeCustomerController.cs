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
    private DishData requestedDish;
    private Inventory playerInventory;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Init(CustomerData customerData, Transform targetSeat, Inventory inventory)
    {
        data = customerData;
        seat = targetSeat;
        playerInventory = inventory;
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

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Press E to serve dish");

            if (Input.GetKeyDown(KeyCode.E) && playerInventory != null)
            {
                TryServeDish(playerInventory);
            }
        }
    }

    private void SitDown()
    {
        agent.isStopped = true;

        if (data.favoriteDishes.Length > 0)
        {
            requestedDish = data.favoriteDishes[Random.Range(0, data.favoriteDishes.Length)];

            thoughtBubble.SetActive(true);
            bubbleDishImage.sprite = requestedDish.dishSprite;

            Debug.Log($"{data.customerName} wants {requestedDish.dishName}!");
        }

        seat = null; // Prevent repeating
    }
    
    public bool TryServeDish(Inventory playerInventory)
    {
        if (requestedDish == null)
        {
            Debug.Log($"{data.customerName} has not requested a dish.");
            return false;
        }

        if (!playerInventory.HasDish(requestedDish))
        {
            Debug.Log($"Player does not have {requestedDish.dishName} to serve.");
            return false;
        }

        // Remove the dish from inventory
        playerInventory.RemoveDish(requestedDish);

        Debug.Log($"{data.customerName} has been served {requestedDish.dishName}!");
        thoughtBubble.SetActive(false);
        requestedDish = null;

        return true;
    }
}