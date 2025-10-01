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
    private int seatIndex = -1;
    private Dish_Data requestedDish;
    private Inventory playerInventory;
    private bool playerInRange = false;
    private bool hasSatDown = false;
    private bool hasRequestedDish = false;
    public event Action<string> OnCustomerLeft;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Init(CustomerData customerData, Transform targetSeat, Inventory inventory, bool spawnSeated = false)
    {
        data = customerData;
        seat = targetSeat;
        seatIndex = Seat_Manager.Instance.GetSeatIndex(targetSeat);
        playerInventory = inventory;

        Transform npc_sprite = transform.Find("Sprite");
        if (npc_sprite != null)
            npc_sprite.GetComponent<SpriteRenderer>().sprite = data.overworldSprite;

        if (spawnSeated)
        {
            // Instantly place at seat and sit
            if (agent.isOnNavMesh)
                agent.Warp(seat.position);
            else
                transform.position = seat.position;

            SitDown(); // immediately mark as seated
        }
        else
        {
            // Normal "walk to seat" behavior
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(seat.position);
                agent.speed = 10f;
            }
        }

        Debug.Log($"Customer {data.customerName} spawned {(spawnSeated ? "already seated" : "at entrance")}.");
    }

    private void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.R) && playerInventory != null)
            {
                if (hasSatDown && !hasRequestedDish)
                {
                    RequestDishAfterDialogue();
                    Game_Events_Manager.Instance.GetOrder();
                }
                else if (hasRequestedDish)
                {
                    TryServeDish(playerInventory);
                    Game_Events_Manager.Instance.ServeCustomer();
                }
            }
        }
        
        // only try to sit if not already sat
        if (!hasSatDown && seat != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
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
        hasSatDown = true;
        // don't null seat anymore, we need it for state saving
        // seat = null; // clear reference to seat so we don't try to sit again
        Debug.Log($"{data.customerName} sat down and is waiting for interaction.");
    }

    #region Dish Picking
    private Dish_Data ChooseWeightedDish()
    {
        var dailyMenu = Choose_Menu_Items.instance?.GetSelectedDishes();
        List<Dish_Data> dailyMenuDishes = new List<Dish_Data>();

        if (dailyMenu != null)
        {
            foreach (var dishEnum in dailyMenu)
            {
                Dish_Data dish = Game_Manager.Instance.dishDatabase.GetDish(dishEnum);
                if (Ingredient_Inventory.Instance.CanMakeDish(dish))
                    dailyMenuDishes.Add(dish);
            }
        }

        List<Dish_Data> favoriteDishes = new List<Dish_Data>();
        if (data.favoriteDishes != null)
        {
            foreach (var dish in data.favoriteDishes)
                if (Ingredient_Inventory.Instance.CanMakeDish(dish))
                    favoriteDishes.Add(dish);
        }

        List<Dish_Data> neutralDishes = new List<Dish_Data>();
        if (data.neutralDishes != null)
        {
            foreach (var dish in data.neutralDishes)
                if (Ingredient_Inventory.Instance.CanMakeDish(dish))
                    neutralDishes.Add(dish);
        }

        // Weighting: Menu (70%), Favorites (20%), Neutral (10%)
        int roll = UnityEngine.Random.Range(0, 100);

        if (roll < 70)
        {
            if (dailyMenuDishes.Count > 0)
                return dailyMenuDishes[UnityEngine.Random.Range(0, dailyMenuDishes.Count)];
            else if (dailyMenu != null && dailyMenu.Count > 0) // fallback: random from menu regardless of ingredients
                return Game_Manager.Instance.dishDatabase.GetDish(dailyMenu[UnityEngine.Random.Range(0, dailyMenu.Count)]);
        }
        else if (roll < 90)
        {
            if (favoriteDishes.Count > 0)
                return favoriteDishes[UnityEngine.Random.Range(0, favoriteDishes.Count)];
            else if (data.favoriteDishes != null && data.favoriteDishes.Length > 0) // fallback
                return data.favoriteDishes[UnityEngine.Random.Range(0, data.favoriteDishes.Length)];
        }
        else
        {
            if (neutralDishes.Count > 0)
                return neutralDishes[UnityEngine.Random.Range(0, neutralDishes.Count)];
            else if (data.neutralDishes != null && data.neutralDishes.Length > 0) // fallback
                return data.neutralDishes[UnityEngine.Random.Range(0, data.neutralDishes.Length)];
        }

        // Global fallback (all lists empty)
        Debug.LogWarning($"[Customer_Controller] {data.customerName} couldn't find any valid dishes. Picking completely random.");
        var allDishes = Game_Manager.Instance.dishDatabase.GetAllDishes();
        return allDishes.Count > 0 ? allDishes[UnityEngine.Random.Range(0, allDishes.Count)] : null;
    }

    /// <summary>
    /// Handles the process of requesting a dish after initial dialogue.
    /// </summary>
    private void RequestDishAfterDialogue()
    {
        // Play filler dialogue
        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
        if (dm != null)
        {
            string fillerKey = $"{data.npcID}.Filler";
            dm.PlayScene(fillerKey, CustomerData.EmotionPortrait.Emotion.Neutral);
        }

        // Pick weighted dish
        requestedDish = ChooseWeightedDish();

        // Show in bubble
        if (requestedDish != null && thoughtBubble != null)
        {
            thoughtBubble.SetActive(true);
            if (bubbleDishImage != null)
                bubbleDishImage.sprite = requestedDish.Image;

            Debug.Log($"{data.customerName} now wants {requestedDish.name}!");
        }
        else
            Debug.LogWarning($"{data.customerName} could not decide on a dish (no valid dishes).");

        hasRequestedDish = true;
    }
    #endregion

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

        // Make sure we're working with the Dish_Tool_Inventory
        var dishInventory = Dish_Tool_Inventory.Instance;
        if (dishInventory == null)
        {
            Debug.LogWarning("No Dish_Tool_Inventory instance found!");
            return false;
        }

        // Check currently selected slot
        Dish_Data selectedDish = dishInventory.GetSelectedDishData();
        if (selectedDish == null)
        {
            Debug.Log("No dish selected to serve.");
            return false;
        }

        if (selectedDish != requestedDish)
        {
            Debug.Log($"Selected dish {selectedDish.name} does not match requested {requestedDish.name}.");
            return false;
        }
       
        // Remove it from inventory
        dishInventory.RemoveSelectedSlot();

        Debug.Log($"{data.customerName} has been served {requestedDish.name}!");
        if (thoughtBubble != null) thoughtBubble.SetActive(false);

        // Record dish served, money earned, and customer served for day summary
        int currencyEarned = Mathf.RoundToInt(selectedDish.price);
        Day_Turnover_Manager.Instance.RecordDishServed(selectedDish, currencyEarned, data.customerName);

        // Prep for dialogue
        (string dialogueKey, string suffix) = GenerateDialogueKey(requestedDish);
        requestedDish = null; // clear after serving

        // Add affection and play a cutscene (if the event has been reached)
        Affection_System.Instance.AddAffection(data, suffix, false);

        // Play dialogue
        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
        if (dm != null && !string.IsNullOrEmpty(dialogueKey))
        {
            var emotion = MapReactionToEmotion(suffix);

            // Assign leave logic to onDialogComplete
            dm.onDialogComplete = () =>
            {
                // Clear to avoid multiple invocations
                dm.onDialogComplete = null;
                LeaveRestaurant();
            };

            dm.PlayScene(dialogueKey, emotion);
            return true;
        }
        else
        {
            // Fallback: leave immediately if no dialogue
            LeaveRestaurant();
            return true;
        }
    }

    public void LeaveRestaurant()
    {
        Debug.Log($"{data.customerName} is leaving the restaurant.");

        if (seat != null)
        {
            Seat_Manager.Instance.FreeSeat(seat);
            seat = null;
        }

        // Pick an exit
        Transform exitPoint = Customer_Exit_Manager.Instance?.GetRandomExit();
        if (exitPoint != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(exitPoint.position);

            // Start coroutine to wait until arrival
            StartCoroutine(LeaveAfterReachingExit(exitPoint));
        }
        else
        {
            // if no exit, destroy immediately instead
            Debug.LogWarning("No exit points defined or agent not on NavMesh. Destroying customer immediately.");
            OnCustomerLeft?.Invoke(data.customerName);
            Destroy(gameObject);
        }
    }

    private IEnumerator LeaveAfterReachingExit(Transform exitPoint)
    {
        // Wait until path is computed
        while (agent.pathPending)
            yield return null;

        // Wait until close enough to exit
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Reached exit
        OnCustomerLeft?.Invoke(data.customerName);
        Destroy(gameObject);
    }


    #region Dialog
    /// <summary>
    /// Generates a dialogue key based on the customer's identity and whether the served dish is liked/neutral/disliked.
    /// Prefer using the portraitData.characterName enum if available, otherwise fall back to data.customerName string.
    /// Example keys produced: "Elf.LikedDish", "Satyr.DislikedDish", "Phrog.NeutralDish"
    /// </summary>
    private (string key, string suffix) GenerateDialogueKey(Dish_Data servedDish)
    {
        if (servedDish == null || data == null)
            return (string.Empty, string.Empty);

        string baseKey = data.npcID.ToString();

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
    private CustomerData.EmotionPortrait.Emotion MapReactionToEmotion(string suffix)
    {
        switch (suffix)
        {
            case "LikedDish":
                return CustomerData.EmotionPortrait.Emotion.Happy;
            case "DislikedDish":
                return CustomerData.EmotionPortrait.Emotion.Disgusted;
            default:
                return CustomerData.EmotionPortrait.Emotion.Neutral;
        }
    }
    #endregion

    #region State Management
    public Customer_State GetState()
    {
        return new Customer_State
        {
            customerName = data.customerName,
            seatIndex = seatIndex, // use stored index
            requestedDishName = requestedDish != null ? requestedDish.name : null,
            hasRequestedDish = hasRequestedDish,
            hasBeenServed = (requestedDish == null && hasRequestedDish)
        };
    }

    /// <summary>
    /// Debug method to force the customer to request a specific dish by name.
    /// </summary>
    public void Debug_ForceRequestDish(string dishName)
    {
        if (string.IsNullOrEmpty(dishName)) return;

        Dish_Data found = null;

        // Look in all known arrays
        foreach (var dish in data.favoriteDishes) if (dish.name == dishName) found = dish;
        foreach (var dish in data.neutralDishes) if (dish.name == dishName) found = dish;
        foreach (var dish in data.dislikedDishes) if (dish.name == dishName) found = dish;

        if (found != null)
        {
            requestedDish = found;
            hasRequestedDish = true;

            if (thoughtBubble != null)
            {
                thoughtBubble.SetActive(true);
                if (bubbleDishImage != null)
                    bubbleDishImage.sprite = requestedDish.Image;
            }
        }
    }
    #endregion
}
