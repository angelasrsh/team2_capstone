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
    private bool playerInRange = false;

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
    agent.speed = 10f;

        Debug.Log($"Customer spawned at {transform.position}, isOnNavMesh = {agent.isOnNavMesh}");
    }

    void Update()
    {
        if (playerInRange)
        {
          Debug.Log("Press R to serve dish");
          Debug.Log($"Player inventory {playerInventory == null}");
          if (Input.GetKeyDown(KeyCode.R) && playerInventory != null)
          {
            Debug.Log("R key pressed, attempting to serve dish");
            TryServeDish(playerInventory);
          }
        }
        
        if (seat != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SitDown();
        }
    }

    void LateUpdate()
    {
        transform.forward = Vector3.forward;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player in range of customer");
        }
    }
    
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player out of range of customer");
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
      Debug.Log("Attempting to serve dish...");
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

        DialogueManager dm = FindObjectOfType<DialogueManager>();
        dm.QueueDialogue("That's my favorite! Thanks! (+10 affection)");

        return true;
    }
}