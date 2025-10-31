using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using Grimoire;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
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
    private InputAction talkAction;
    private bool hasSatDown = false;
    private bool hasRequestedDish = false;
    public event Action<string> OnCustomerLeft;

    // Animation
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform spriteTransform;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRebind;
        TryBindInput();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedRebind;
    }

    private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode)
    {
        TryBindInput();
    }

    private void TryBindInput()
    {
        PlayerInput playerInput = null;

        // Preferred: get from Game_Manager
        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        // Fallback: find from Player_Input_Controller
        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[Customer_Controller] No PlayerInput found to bind actions.");
            return;
        }

        talkAction = playerInput.actions["Talk"];
        if (talkAction == null)
            Debug.LogWarning("[Customer_Controller] Could not find 'Talk' action in PlayerInput!");
        else
            talkAction.Enable();

        Debug.Log("[Customer_Controller] Input bound successfully.");
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // spriteTransform = transform.Find("Sprite");

        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            animator = spriteTransform.GetComponent<Animator>();
        }
    }

    public void Init(CustomerData customerData, Transform targetSeat, Inventory inventory, bool spawnSeated = false)
    {
        data = customerData;
        seat = targetSeat;
        seatIndex = Seat_Manager.Instance.GetSeatIndex(targetSeat);
        playerInventory = inventory;

        // Set sprite
        Transform npc_sprite = spriteTransform;
        if (npc_sprite != null)
            npc_sprite.GetComponent<SpriteRenderer>().sprite = data.overworldSprite;

        // Set walk animation  

        if (animator != null && data.walkAnimatorController != null)
            animator.runtimeAnimatorController = data.walkAnimatorController;

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
            if (talkAction.WasPerformedThisFrame() && playerInventory != null)
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

        HandleWalkAnimation();
    }

    void LateUpdate()
    {
        transform.forward = Vector3.forward;
    }

    private void HandleWalkAnimation()
    {
        if (animator == null || spriteRenderer == null || agent == null)
            return;

        float speed = agent.velocity.magnitude;

        if (animator.runtimeAnimatorController != null)
        {
            if (animator.HasParameterOfType("speed", AnimatorControllerParameterType.Float))
                animator.SetFloat("speed", speed);

            if (animator.HasParameterOfType("isSeated", AnimatorControllerParameterType.Bool))
                animator.SetBool("isSeated", hasSatDown);
        }

        // Flip sprite based on movement direction
        if (speed > 0.05f)
        {
            bool faceRight = agent.velocity.x > 0.01f;
            spriteRenderer.flipX = faceRight;

            if (animator.runtimeAnimatorController != null &&
                animator.HasParameterOfType("facingRight", AnimatorControllerParameterType.Bool))
            {
                animator.SetBool("facingRight", faceRight);
            }
        }
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
        List<Dish_Data> favoriteDishes = new List<Dish_Data>();
        List<Dish_Data> neutralDishes = new List<Dish_Data>();

        // --- Daily Menu Section ---
        if (dailyMenu != null)
        {
            foreach (var dishEnum in dailyMenu)
            {
                Dish_Data dish = Game_Manager.Instance.dishDatabase.GetDish(dishEnum);
                if (Ingredient_Inventory.Instance.CanMakeDish(dish))
                    dailyMenuDishes.Add(dish);
            }

            // Safety check before using random range
            if (dailyMenuDishes.Count > 0)
                return dailyMenuDishes[UnityEngine.Random.Range(0, dailyMenuDishes.Count)];
        }

        // --- Favorites ---
        if (data.favoriteDishes != null)
        {
            foreach (var dish in data.favoriteDishes)
                if (Ingredient_Inventory.Instance.CanMakeDish(dish))
                    favoriteDishes.Add(dish);
        }

        // --- Neutral ---
        if (data.neutralDishes != null)
        {
            foreach (var dish in data.neutralDishes)
                if (Ingredient_Inventory.Instance.CanMakeDish(dish))
                    neutralDishes.Add(dish);
        }

        // --- Weighted Roll ---
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll < 70 && dailyMenuDishes.Count > 0)
            return dailyMenuDishes[UnityEngine.Random.Range(0, dailyMenuDishes.Count)];
        else if (roll < 90 && favoriteDishes.Count > 0)
            return favoriteDishes[UnityEngine.Random.Range(0, favoriteDishes.Count)];
        else if (neutralDishes.Count > 0)
            return neutralDishes[UnityEngine.Random.Range(0, neutralDishes.Count)];

        // --- Fallback: Any cookable dish ---
        var allCookable = new List<Dish_Data>();
        foreach (var dish in Game_Manager.Instance.dishDatabase.GetAllDishes())
        {
            if (Ingredient_Inventory.Instance.CanMakeDish(dish))
                allCookable.Add(dish);
        }

        if (allCookable.Count > 0)
            return allCookable[UnityEngine.Random.Range(0, allCookable.Count)];

        // --- Final fallback ---
        Debug.LogWarning($"[Customer_Controller] {data.customerName} found no cookable dishes. Picking truly random.");
        var allDishesFallback = Game_Manager.Instance.dishDatabase.GetAllDishes();
        return allDishesFallback.Count > 0
            ? allDishesFallback[UnityEngine.Random.Range(0, allDishesFallback.Count)]
            : null;
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

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
            // Debug.Log("Player in range of customer");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
            // Debug.Log("Player out of range of customer");
        }
    }

    /// <summary>
    /// Attempts to serve the customer's requested dish using the player's inventory.
    /// Returns true if a dish was successfully served.
    /// </summary>
    public bool TryServeDish(Inventory playerInventory)
    {
        Debug.Log("Attempting to serve dish...");
        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();

        if (requestedDish == null)
        {
            Debug.Log($"{data.customerName} has not requested a dish.");
            return false;
        }

        // Make sure we're working with Dish_Tool_Inventory
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

        bool isFailedDish = selectedDish.dishType == Dish_Data.Dishes.Failed_Dish;

        // Only enforce exact match if it's NOT the failed dish
        if (selectedDish != requestedDish && !isFailedDish)
        {
            Debug.Log($"Selected dish {selectedDish.name} does not match requested {requestedDish.name}.");
            return false;
        }

        // Remove served dish from inventory
        dishInventory.RemoveSelectedSlot();

        Debug.Log($"{data.customerName} has been served {selectedDish.name}!");
        Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.orderServed, 0.75f);
        if (thoughtBubble != null) thoughtBubble.SetActive(false);

        // Record the served dish
        int currencyEarned = Mathf.RoundToInt(selectedDish.price);
        Day_Turnover_Manager.Instance.RecordDishServed(selectedDish, currencyEarned, data.customerName);

        // Check for special dishes
        if (selectedDish.name == "One-Day Blinding Stew")
        {
            if (dm != null)
            {
                string baseKey = $"{data.npcID}.BlindingStew";
                string resolvedKey = dm.ResolveDialogKey(baseKey);

                var emotion = CustomerData.EmotionPortrait.Emotion.Surprised;

                dm.onDialogComplete = () =>
                {
                    dm.onDialogComplete = null;
                    LeaveRestaurant();
                };
                dm.PlayScene(resolvedKey, emotion);
            }
            requestedDish = null;
            return true;
        }


        // Determine dialogue + affection logic
        (string dialogueKey, string suffix) = isFailedDish
            ? GenerateFailedDishDialogueKey()
            : GenerateDialogueKey(selectedDish);

        requestedDish = null;

        // Add affection (failed dish always negative)
        Affection_System.Instance.AddAffection(data, suffix, false);

        // Play dialogue & leave after completion
        if (dm != null && !string.IsNullOrEmpty(dialogueKey))
        {
            var emotion = MapReactionToEmotion(suffix);

            dm.onDialogComplete = () =>
            {
                dm.onDialogComplete = null;
                LeaveRestaurant();
            };

            dm.PlayScene(dialogueKey, emotion);
            return true;
        }
        else
        {
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

            // Save in restaurant state and save manager
            Restaurant_State.Instance?.SaveCustomers();
            Save_Manager.instance?.AutoSave();
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

        // Save in restaurant state and save manager
        Restaurant_State.Instance?.SaveCustomers();
        Save_Manager.instance?.AutoSave();
    }


    #region Dialog
    /// <summary>
    /// Generates a dialogue key based on the customer's identity and whether the served dish is liked/neutral/disliked.
    /// Special-case dishes (like One-Day Blinding Stew) are checked first so they can override normal favorite/disliked logic.
    /// </summary>
    private (string key, string suffix) GenerateDialogueKey(Dish_Data servedDish)
    {
        if (servedDish == null || data == null)
            return (string.Empty, string.Empty);

        string baseKey = data.npcID.ToString();

        // 1) Special-case dishes first (they must override favorites/dislikes)
        if (servedDish.dishType == Dish_Data.Dishes.Blinding_Stew)
        {
            return ($"{baseKey}.BlindingStew", "BlindingStew");
        }

        // 2) Otherwise use favorite/disliked/neutral logic
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

    private (string key, string suffix) GenerateFailedDishDialogueKey()
    {
        string baseKey = data.npcID.ToString();
        string suffix = "DislikedDish";
        return ($"{baseKey}.{suffix}", suffix);
    }
    #endregion


    #region State Management
    public Customer_State GetState()
    {
        Debug.Log("[Customer_Controller]: customer: " + data.customerName + " being saved.");
        if (requestedDish == null)
            Debug.Log("[Customer_Controller]: requested dish is null");
        else
            Debug.Log("[Customer_Controller]: requested dish: " + requestedDish.name + " being saved.");
        Debug.Log("[Customer_Controller]: hasRequestedDish: " + hasRequestedDish + ".");
        Debug.Log("[Customer_Controller]: served: " + (requestedDish == null && hasRequestedDish) + ".");
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
        // foreach (var dish in data.favoriteDishes) if (dish.name == dishName) found = dish;
        // foreach (var dish in data.neutralDishes) if (dish.name == dishName) found = dish;
        // foreach (var dish in data.dislikedDishes) if (dish.name == dishName) found = dish;
        foreach (var dish in Game_Manager.Instance.dishDatabase.GetAllDishes()) if (dish.name == dishName) found = dish;

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

public static class AnimatorExtensions
{
    public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
    {
        if (self == null) return false;
        foreach (var param in self.parameters)
        {
            if (param.type == type && param.name == name)
                return true;
        }
        return false;
    }
}

