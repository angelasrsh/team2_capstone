using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Customer_Controller : MonoBehaviour
{
    public CustomerData data;

    [SerializeField] private GameObject thoughtBubble;
    [SerializeField] private Image bubbleDishImage;

    // Internal components
    private NavMeshAgent agent;
    private Transform seat;
    private Dish_Data requestedDish;
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
        Transform npc_sprite = transform.Find("Sprite");
        if (npc_sprite != null)
            npc_sprite.GetComponent<SpriteRenderer>().sprite = data.overworldSprite;

        Debug.Log($"Customer spawned at {transform.position}, isOnNavMesh = {agent.isOnNavMesh}");
    }

    void Update()
    {
        if (playerInRange)
        {
            // keep some debugging to confirm the input path
            // Press R to serve dish
            if (Input.GetKeyDown(KeyCode.R) && playerInventory != null)
            {
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

        if (data.favoriteDishes != null && data.favoriteDishes.Length > 0)
        {
            requestedDish = data.favoriteDishes[UnityEngine.Random.Range(0, data.favoriteDishes.Length)];

            if (thoughtBubble != null)
            {
                thoughtBubble.SetActive(true);
                if (bubbleDishImage != null && requestedDish != null)
                    bubbleDishImage.sprite = requestedDish.Image;
            }

            Debug.Log($"{data.customerName} wants {requestedDish?.name ?? "Unknown"}!");
        }

        seat = null; // Prevent repeating
    }

    /// <summary>
    /// Attempts to serve the customer's requested dish using the player's inventory.
    /// Returns true if a dish was successfully served.
    /// </summary>
    public bool TryServeDish(Inventory playerInventory)
    {
        Debug.Log("Attempting to serve dish...");

        if (requestedDish == null)
        {
            Debug.Log($"{data.customerName} has not requested a dish.");
            return false;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("TryServeDish called with null playerInventory.");
            return false;
        }

        if (!playerInventory.HasItem(requestedDish))
        {
            Debug.Log($"Player does not have {requestedDish.name} to serve.");
            return false;
        }

        // Remove the dish
        int removed = playerInventory.RemoveResources(requestedDish, 1);
        if (removed <= 0)
        {
            Debug.Log($"[Customer_Controller] Error: no {requestedDish.name} served!");
            return false;
        }

        Debug.Log($"{data.customerName} has been served {requestedDish.name}!");
        if (thoughtBubble != null) thoughtBubble.SetActive(false);

        // Get dialogue key + suffix
        (string dialogueKey, string suffix) = GenerateDialogueKey(requestedDish);

        requestedDish = null; // clear after generating

        // Play dialog with forced emotion
        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
        if (dm != null && !string.IsNullOrEmpty(dialogueKey))
        {
            var emotion = MapReactionToEmotion(suffix);
            Debug.Log($"Playing dialogue key: {dialogueKey} with emotion {emotion}");
            dm.PlayScene(dialogueKey, emotion);
        }
        else
        {
            Debug.LogWarning($"Dialogue_Manager missing or dialogueKey empty for {data.customerName}");
        }

        return true;
    }


    /// <summary>
    /// Generates a dialogue key based on the customer's identity and whether the served dish is liked/neutral/disliked.
    /// Prefer using the portraitData.characterName enum if available, otherwise fall back to data.customerName string.
    /// Example keys produced: "Elf.LikedDish", "Satyr.DislikedDish", "Phrog.NeutralDish"
    /// </summary>
   private (string key, string suffix) GenerateDialogueKey(Dish_Data servedDish)
    {
        if (servedDish == null || data == null)
            return (string.Empty, string.Empty);

        string baseKey = data.customerName;

        string suffix = "NeutralDish";
        if (data.favoriteDishes != null && Array.Exists(data.favoriteDishes, d => d == servedDish))
        {
            suffix = "LikedDish";
        }
        else if (data.dislikedDishes != null && Array.Exists(data.dislikedDishes, d => d == servedDish))
        {
            suffix = "DislikedDish";
        }

        return ($"{baseKey}.{suffix}", suffix);
    }

    /// <summary>
    /// Maps the reaction type to a portrait emotion.
    /// </summary>
    private Character_Portrait_Data.EmotionPortrait.Emotion MapReactionToEmotion(string suffix)
    {
        switch (suffix)
        {
            case "LikedDish":
                return Character_Portrait_Data.EmotionPortrait.Emotion.Happy;
            case "DislikedDish":
                return Character_Portrait_Data.EmotionPortrait.Emotion.Disgusted;
            default:
                return Character_Portrait_Data.EmotionPortrait.Emotion.Neutral;
        }
    }
}
