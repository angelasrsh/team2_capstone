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
    private bool firstIntroDialogPlaying = false;

    // Tutorial customer
    private bool tutorialDialogPlayed => Player_Progress.Instance.TutorialCustomerDialogDone;
    [HideInInspector] public bool isTutorialCustomer = false; 
    private const float tutorialTalkDistance = 5.0f;

    private Player_Controller player;
    Dialogue_Manager dm;
    Dialog_UI_Manager duim;

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

        // Attach dialogue manager and dialogue UI manager
        dm = FindObjectOfType<Dialogue_Manager>();
        duim = FindObjectOfType<Dialog_UI_Manager>();
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
        // Tutorial dialogue check
        if (isTutorialCustomer &&
        Player_Progress.Instance.InGameplayTutorial &&
        !tutorialDialogPlayed &&
        PlayerIsCloseEnoughForTutorial())
        {
            Player_Progress.Instance.MarkTutorialDialogDone();
            PlayTutorialDialogue();
        }

        // Standard interaction check
        if (playerInRange)
        {
            // --- Normal Talk Logic ---
            if (talkAction != null && talkAction.WasPerformedThisFrame() && playerInventory != null)
            {
                if (hasSatDown && !hasRequestedDish)
                {
                    if (Player_Progress.Instance != null && !Player_Progress.Instance.HasMetNPC(data.npcID) && !isTutorialCustomer)
                    {
                        PlayFirstMeetingDialogue();
                        return;
                    }

                    if (TryPlayRingReturnDialogue())
                        return;

                    RequestDishAfterDialogue();
                    Game_Events_Manager.Instance.GetOrder();
                }
                else if (hasRequestedDish && !duim.IsOpen)
                {
                    bool served = TryServeDish(playerInventory);

                    if (served)
                    {
                        Game_Events_Manager.Instance.ServeCustomer();
                    }
                }
            }
        }

        // only try to sit if not already sat
        if (!hasSatDown && seat != null && agent.isOnNavMesh &&
                !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            SitDown();

        HandleWalkAnimation();
    }

    void LateUpdate() => transform.forward = Vector3.forward;

    private bool PlayerIsCloseEnoughForTutorial()
    {
        if (player == null)
            player = FindObjectOfType<Player_Controller>();

        if (player == null)
            return true;  // fallback, never block

        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist > tutorialTalkDistance)
            return false;

        return true;
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
        Debug.Log($"{data.customerName} sat down at seat index {seatIndex}.");

        // immediately save state after sitting down
        Restaurant_State.TryAutoSave();
        Save_Manager.instance?.AutoSave();
    }


    #region Dish Picking
    private Dish_Data ChooseDailyMenuDish()
    {
        var dailyMenuEnums = Choose_Menu_Items.instance?.GetSelectedDishes();

        if (dailyMenuEnums == null || dailyMenuEnums.Count == 0)
        {
            Debug.LogWarning($"[{data.customerName}] No daily menu dishes set — picking random fallback dish.");
            var allDishes = Game_Manager.Instance.dishDatabase.GetAllDishes();
            return allDishes.Count > 0
                ? allDishes[UnityEngine.Random.Range(0, allDishes.Count)]
                : null;
        }

        // Always pick a random dish from today's menu — no ingredient checks
        Dish_Data.Dishes chosenEnum = dailyMenuEnums[UnityEngine.Random.Range(0, dailyMenuEnums.Count)];
        Dish_Data chosenDish = Game_Manager.Instance.dishDatabase.GetDish(chosenEnum);

        Debug.Log($"[{data.customerName}] picked {chosenDish.name} from today's menu.");
        return chosenDish;
    }

    // private Dish_Data ChooseWeightedDish()
    // {
    //     var dailyMenu = Choose_Menu_Items.instance?.GetSelectedDishes();
    //     List<Dish_Data> dailyMenuDishes = new List<Dish_Data>();
    //     List<Dish_Data> favoriteDishes = new List<Dish_Data>();
    //     List<Dish_Data> neutralDishes = new List<Dish_Data>();

    //     // --- Daily Menu Section ---
    //     if (dailyMenu != null)
    //     {
    //         foreach (var dishEnum in dailyMenu)
    //         {
    //             Dish_Data dish = Game_Manager.Instance.dishDatabase.GetDish(dishEnum);
    //             if (Ingredient_Inventory.Instance.CanMakeDish(dish))
    //                 dailyMenuDishes.Add(dish);
    //         }

    //         // Safety check before using random range
    //         if (dailyMenuDishes.Count > 0)
    //             return dailyMenuDishes[UnityEngine.Random.Range(0, dailyMenuDishes.Count)];
    //     }

    //     // --- Favorites ---
    //     if (data.favoriteDishes != null)
    //     {
    //         foreach (var dish in data.favoriteDishes)
    //             if (Ingredient_Inventory.Instance.CanMakeDish(dish))
    //                 favoriteDishes.Add(dish);
    //     }

    //     // --- Neutral ---
    //     if (data.neutralDishes != null)
    //     {
    //         foreach (var dish in data.neutralDishes)
    //             if (Ingredient_Inventory.Instance.CanMakeDish(dish))
    //                 neutralDishes.Add(dish);
    //     }

    //     // --- Weighted Roll ---
    //     int roll = UnityEngine.Random.Range(0, 100);
    //     if (roll < 70 && dailyMenuDishes.Count > 0)
    //         return dailyMenuDishes[UnityEngine.Random.Range(0, dailyMenuDishes.Count)];
    //     else if (roll < 90 && favoriteDishes.Count > 0)
    //         return favoriteDishes[UnityEngine.Random.Range(0, favoriteDishes.Count)];
    //     else if (neutralDishes.Count > 0)
    //         return neutralDishes[UnityEngine.Random.Range(0, neutralDishes.Count)];

    //     // --- Fallback: Any cookable dish ---
    //     var allCookable = new List<Dish_Data>();
    //     foreach (var dish in Game_Manager.Instance.dishDatabase.GetAllDishes())
    //     {
    //         if (Ingredient_Inventory.Instance.CanMakeDish(dish))
    //             allCookable.Add(dish);
    //     }

    //     if (allCookable.Count > 0)
    //         return allCookable[UnityEngine.Random.Range(0, allCookable.Count)];

    //     // --- Final fallback ---
    //     Debug.LogWarning($"[Customer_Controller] {data.customerName} found no cookable dishes. Picking truly random.");
    //     var allDishesFallback = Game_Manager.Instance.dishDatabase.GetAllDishes();
    //     return allDishesFallback.Count > 0
    //         ? allDishesFallback[UnityEngine.Random.Range(0, allDishesFallback.Count)]
    //         : null;
    // }

    /// <summary>
    /// Handles the process of requesting a dish after initial dialogue.
    /// </summary>
    private void RequestDishAfterDialogue()
    {
        // Unlock NPC in journal if not already done
        if (Player_Progress.Instance != null && !Player_Progress.Instance.IsNPCUnlocked(data.npcID))
            Player_Progress.Instance.UnlockNPC(data.npcID);

        // Play filler dialogue
        if (dm == null)
            dm = FindObjectOfType<Dialogue_Manager>();

        if (dm != null)
        {
            dm.PlayScene($"{data.npcID}.Filler", CustomerData.EmotionPortrait.Emotion.Neutral);
            if (player == null)
                player = FindObjectOfType<Player_Controller>();
            if (player != null)
                player.DisablePlayerMovement();
        }

        // Pick a daily menu dish (always)
        requestedDish = ChooseDailyMenuDish();

        // Show in thought bubble
        if (requestedDish != null && thoughtBubble != null)
        {
            thoughtBubble.SetActive(true);
            if (bubbleDishImage != null)
                bubbleDishImage.sprite = requestedDish.Image;

            Debug.Log($"{data.customerName} now wants {requestedDish.name}!");
        }
        else
            Debug.LogWarning($"{data.customerName} could not decide on a dish!");

        hasRequestedDish = true;

        // Save request in restaurant state and save manager
        Restaurant_State.Instance?.SaveCustomers();
        Save_Manager.instance?.AutoSave();
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
        if (dm == null)
            dm = FindObjectOfType<Dialogue_Manager>();

        if (requestedDish == null)
        {
            Debug.Log($"{data.customerName} has not requested a dish.");
            return false;
        }

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

        // Check if served dish matches requested dish
        bool isFailedDish = selectedDish.dishType == Dish_Data.Dishes.Failed_Dish;
        bool wrongDish = selectedDish != requestedDish && !isFailedDish;

        // Remove served dish from inventory
        dishInventory.RemoveSelectedSlot();

        Debug.Log($"{data.customerName} has been served {selectedDish.name}!");
        Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.orderServed, 0.75f);
        if (thoughtBubble != null) thoughtBubble.SetActive(false);

        // Save in restaurant state and save manager
        Restaurant_State.Instance?.SaveCustomers();
        Save_Manager.instance?.AutoSave();

        // Check if player served the wrong dish
        if (wrongDish)
            Debug.Log($"[{data.customerName}] was served the WRONG dish: expected {requestedDish.name}, got {selectedDish.name}");

        // Record the served dish
        int currencyEarned = Mathf.RoundToInt(selectedDish.price);
        if (wrongDish)
            currencyEarned = Mathf.RoundToInt(currencyEarned * 0.5f);  // halve payment for wrong dish

        Player_Progress.Instance.AddMoney(currencyEarned);
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

        // --- Tutorial completion: if this is the tutorial customer and the player served a valid dish, end tutorial mode ---
        if (isTutorialCustomer && Player_Progress.Instance != null && Player_Progress.Instance.InGameplayTutorial)
        {
            Debug.Log("[Customer_Controller] Tutorial customer served correctly — ending gameplay tutorial.");

            Player_Progress.Instance.MarkTutorialDialogDone();

            // Turn off tutorial mode 
            Player_Progress.Instance.SetGameplayTutorial(false);
            Player_Progress.Instance.SetIntroPlayed(true);

             // Persist immediately
            Save_Manager.instance?.SaveGameData();
        }

        // Determine dialogue + affection logic
        if (isFailedDish)
        {
            (string dialogueKey, string suffix) = GenerateFailedDishDialogueKey();
            Affection_System.Instance.AddAffection(data, suffix, false);
            PlayCustomerDialogue(dialogueKey, suffix);
        }
        else if (wrongDish)
        {
            (string dialogueKey, string suffix) = GenerateWrongDishDialogueKey();
            Affection_System.Instance.AddAffection(data, suffix, true); // maybe partial affection
            PlayCustomerDialogue(dialogueKey, suffix);
        }
        else
        {
            (string dialogueKey, string suffix) = GenerateDialogueKey(selectedDish);

            // Check if this dish is both requested and on today's daily menu
            bool onDailyMenu = false;
            var dailyMenuEnums = Choose_Menu_Items.instance?.GetSelectedDishes();
            if (dailyMenuEnums != null)
            {
                foreach (var menuDishEnum in dailyMenuEnums)
                {
                    Dish_Data dailyDish = Game_Manager.Instance.dishDatabase.GetDish(menuDishEnum);
                    if (dailyDish == selectedDish)
                    {
                        onDailyMenu = true;
                        break;
                    }
                }
            }

            // If it's the requested dish AND on the daily menu, give favorite affection
            if (onDailyMenu && selectedDish == requestedDish)
            {
                Debug.Log($"[{data.customerName}] received a daily menu bonus for {selectedDish.name}! " +
                        $"Giving favorite-dish affection (dialog remains {suffix}).");

                // Treat as favorite affection, but don't alter dialog
                Affection_System.Instance.AddAffection(data, "LikedDish", false);
            }
            else
            {
                // Normal affection behavior
                Affection_System.Instance.AddAffection(data, suffix, false);
            }

            PlayCustomerDialogue(dialogueKey, suffix);
        }

        requestedDish = null;
        return true;
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
    /// Attempts to play the ring return dialogue for Asper if the player has the ring item in their inventory.
    /// If successful, removes the ring from inventory and marks it as collected.
    /// </summary>
    /// <returns></returns>
    private void PlayTutorialDialogue()
    {
        if (dm == null)
            dm = FindObjectOfType<Dialogue_Manager>();

        if (dm == null)
        {
            Debug.LogWarning("No Dialogue_Manager found for tutorial dialog.");
            return;
        }

        string key = "Elf.Get_Order_Asper"; 
        dm.PlayScene(key, CustomerData.EmotionPortrait.Emotion.Neutral);
    }
    
    private void PlayFirstMeetingDialogue()
    {
        if (dm == null)
            dm = FindObjectOfType<Dialogue_Manager>();
        if (dm == null)
        {
            Debug.LogWarning("Dialogue_Manager not found for first meeting.");
            return;
        }

        if (player != null)
                player.DisablePlayerMovement();

        dm.onDialogComplete = () =>
        {
            dm.onDialogComplete = null;
            Player_Progress.Instance.MarkNPCIntroduced(data.npcID);
            firstIntroDialogPlaying = false;
            // if (player == null)
            //     player = FindObjectOfType<Player_Controller>();
            // if (player != null)
            //     player.EnablePlayerMovement();

            // After introduction, continue like normal
            RequestDishAfterDialogue();
            Game_Events_Manager.Instance.GetOrder();
        };

        if (firstIntroDialogPlaying)
            return;

        // e.g. "Elf.Intro"
        firstIntroDialogPlaying = true;
        dm.PlayScene($"{data.npcID}.Intro", CustomerData.EmotionPortrait.Emotion.Neutral);
    }
    
    private bool TryPlayRingReturnDialogue()
    {
        if (Affection_Event_Item_Tracker.instance == null)
        {
            Debug.LogWarning($"[{data.customerName}] Tracker instance missing.");
            return false;
        }

        if (data == null)
        {
            Debug.LogWarning("[RingReturn] CustomerData missing on NPC.");
            return false;
        }
        var tracker = Affection_Event_Item_Tracker.instance;

        // Find the relevant entry in the tracker
        var entry = tracker.items.Find(i => i.npc != null && i.npc.npcID == data.npcID);
        if (entry == null)
        {
            Debug.LogWarning($"[{data.customerName}] No event item entry found in tracker.");
            return false;
        }

        Debug.Log($"[DEBUG] Tracker entry for {data.customerName}: " +
                $"Unlocked={entry.unlocked}, Collected={entry.collected}, Item={entry.eventItem?.Name}");

        // Skip if item hasn’t been unlocked yet
        if (!entry.unlocked)
        {
            Debug.Log($"[{data.customerName}] Event item not yet unlocked — skipping ring return dialogue.");
            return false;
        }

        // Check if player has the ring item in inventory
        var inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning($"[{data.customerName}] Inventory missing in scene.");
            return false;
        }

        if (entry.eventItem == null)
        {
            Debug.LogWarning($"[{data.customerName}] entry.eventItem missing!");
            return false;
        }

        bool hasItem = inventory.HasItem(entry.eventItem);
        Debug.Log($"[{data.customerName}] Checking inventory for {entry.eventItem.Name} → {hasItem}");

        // Only play return dialogue if the player currently has the item
        if (!hasItem)
        {
            Debug.Log($"[{data.customerName}] Player does not currently have the ring — using normal dialogue instead.");
            return false;
        }

        // Trigger ring return dialogue
        if (dm == null)
            dm = FindObjectOfType<Dialogue_Manager>();
        if (dm == null)
        {
            Debug.LogWarning("[RingReturn] Dialogue_Manager missing.");
            return false;
        }

        dm.onDialogComplete = () =>
        {
            dm.onDialogComplete = null;
            if (player == null)
                player = FindObjectOfType<Player_Controller>();
            if (player != null)
                player.EnablePlayerMovement();

            // Remove the item from the player's inventory and mark collected
            inventory.RemoveResources(entry.eventItem, 1);
            Ingredient_Inventory.Instance.RemoveResources(entry.eventItem, 1);

            if (!entry.collected)
            {
                tracker.MarkCollected(entry);
                Debug.Log($"[{data.customerName}] Ring returned and marked collected.");
            }
            else
                Debug.Log($"[{data.customerName}] Ring was already marked collected — just removing from inventory.");
        };

        Debug.Log($"[{data.customerName}] Playing ring return dialogue scene: {data.npcID}.RingReturn");
        dm.PlayScene($"{data.npcID}.RingReturn", CustomerData.EmotionPortrait.Emotion.Happy);
        return true;
    }


    /// <summary>
    /// Generates a dialogue key based on the customer's identity and whether the served dish is liked/neutral/disliked.
    /// Special-case dishes (like One-Day Blinding Stew) are checked first so they can override normal favorite/disliked logic.
    /// </summary>
    private (string key, string suffix) GenerateDialogueKey(Dish_Data servedDish)
    {
        if (servedDish == null || data == null)
            return (string.Empty, string.Empty);

        string baseKey = data.npcID.ToString();

        // 1. Special-case dishes first (they must override favorites/dislikes)
        if (servedDish.dishType == Dish_Data.Dishes.Blinding_Stew)
        {
            return ($"{baseKey}.BlindingStew", "BlindingStew");
        }

        // 2. Otherwise use favorite/disliked/neutral logic
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

    private void PlayCustomerDialogue(string dialogueKey, string suffix)
    {
        if (dm == null)
            dm = FindObjectOfType<Dialogue_Manager>();
        if (dm == null || string.IsNullOrEmpty(dialogueKey))
        {
            LeaveRestaurant();
            return;
        }

        var emotion = MapReactionToEmotion(suffix);
        dm.onDialogComplete = () =>
        {
            dm.onDialogComplete = null;
            if (player == null)
                player = FindObjectOfType<Player_Controller>();
            if (player != null)
                player.EnablePlayerMovement();
            LeaveRestaurant();
        };
        dm.PlayScene(dialogueKey, emotion);
    }

    private (string key, string suffix) GenerateFailedDishDialogueKey()
    {
        string baseKey = data.npcID.ToString();
        string suffix = "DislikedDish";
        return ($"{baseKey}.{suffix}", suffix);
    }

    private (string key, string suffix) GenerateWrongDishDialogueKey()
    {
        string baseKey = data.npcID.ToString();
        return ($"{baseKey}.WrongDish", "WrongDish");
    }
    #endregion


    #region State Management
    public Customer_State GetState()
    {
        // Debug.Log("[Customer_Controller]: customer: " + data.customerName + " being saved.");
        if (requestedDish == null)
            Debug.Log("[Customer_Controller]: requested dish is null");
        else
            Debug.Log("[Customer_Controller]: requested dish: " + requestedDish.name + " being saved.");

        // Debug.Log("[Customer_Controller]: hasRequestedDish: " + hasRequestedDish + ".");
        // Debug.Log("[Customer_Controller]: served: " + (requestedDish == null && hasRequestedDish) + ".");

        return new Customer_State
        {
            customerName = data.customerName,
            seatIndex = seatIndex, // use stored index
            requestedDishName = requestedDish != null ? requestedDish.name : null,
            hasRequestedDish = hasRequestedDish,
            hasBeenServed = (requestedDish == null && hasRequestedDish),
            isTutorialCustomer = this.isTutorialCustomer
        };
    }
    #endregion
}

#region Animator Extensions
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
#endregion

