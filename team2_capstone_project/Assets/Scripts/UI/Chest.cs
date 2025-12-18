using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Grimoire;
using TMPro;

public class Chest : MonoBehaviour
{
    public static Chest Instance;
    public static event System.Action<bool> OnChestOpenChanged;
    private bool isPlayerInRange = false;
    public bool chestOpen = false; 
    [SerializeField] private GameObject ChestUI;
    [SerializeField] private GameObject grid;
    [SerializeField] public RectTransform redZone; // only here so that drag_all can easily find redZone even when it's inactive
    [SerializeField] public List<Chest_Item> itemsInChest; // assign 18 slots in inspector
    public string TutorialDialogueKey;

    // Tracking for tutorial
    public bool hasPlayedTutorial;

    private bool isMobile = false;
    [SerializeField] private Button chestCloseButton;


    
    [Header("Player Input Info")]
    private Player_Controller player;
    private InputAction interactAction, closeAction;
    private PlayerInput playerInput;

    void Awake()
    {
      if (Instance == null)
        Instance = this;
      else
        Debug.LogWarning("[Chest] Multiple Chest instances in the scene!");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Game_Manager.Instance == null)
        {
            Debug.LogWarning("[Chest]: Game_Manager instance is null!");
            return;
        }
        playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
        interactAction = playerInput.actions["Interact"]; // From Player Input Map
        closeAction = playerInput.actions.FindAction("CloseInteract", true); // From UI Input Map
        }

        if (ChestUI == null)
        {
            Debug.LogWarning("[Chest]: Chest UI is not set in inspector!");
        }
        redZone = GameObject.Find("ChestRedZone").GetComponent<RectTransform>();

        if (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld)
          isMobile = true;
        else
          isMobile = false;

        if (isMobile)
          chestCloseButton.gameObject.SetActive(true);

        CreateChestSlots();
        CloseChestUI();
        
        // Try to load save data for this scene
        var save = Save_Manager.GetGameData();
        if (save != null && save.chestData != null)
        LoadChestData(save.chestData);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlayerInRange || !Game_Manager.Instance.UIManager.CanProcessInput() || player == null)
            return;
      
    // Only process input if player is inside trigger and interact pressed once
    if (interactAction.WasPerformedThisFrame() && !Game_Manager.Instance.UIManager.pauseMenuOn && !chestOpen)
        {
          Debug.Log("opening Chest");
          OpenChestUI();
        } else if (closeAction.WasPerformedThisFrame() && chestOpen)
            CloseChestUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            player = other.GetComponent<Player_Controller>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            player = null;
        }
    }
    private void OpenChestUI()
    {
        ChestUI.SetActive(true);
        chestOpen = true;
        Game_Manager.Instance.UIManager.OpenUI();
        Debug.Log("Chest ui opened");
        // Notify draggables to allow drag when trash open
        OnChestOpenChanged?.Invoke(chestOpen);

        // Pause player movement
        player?.DisablePlayerMovement();

        // #if TUTORIAL_ENABLE
        // Tutorial
        if (!hasPlayedTutorial)
        {
            Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
            dm.PlayScene(TutorialDialogueKey);
            hasPlayedTutorial = true;
        }
        // #endif
    }

    public void CloseChestUI()
    {
        ChestUI.SetActive(false);
        if (chestOpen)
            Game_Manager.Instance.UIManager.CloseUI();
        chestOpen = false;

        // Allow player movement
        player?.EnablePlayerMovement();

        // Notify draggables to disable drag when trash closed
        OnChestOpenChanged?.Invoke(chestOpen);
    }


    /// <summary>
    /// Sets the data for each item slot in the trash. These slots should have empty images, empty amounts.
    /// </summary>
    private void CreateChestSlots()
    {
      itemsInChest.Clear();
      foreach (Transform slot in grid.transform)
      {
        Chest_Item chestItem = slot.gameObject.GetComponent<Chest_Item>();
        if (chestItem == null)
          chestItem = slot.gameObject.AddComponent<Chest_Item>();
        chestItem.Setup(slot);
        itemsInChest.Add(chestItem);
      }
    }

  /// <summary>
  /// Adds item dropped into trash into the trash slots. This method assumes you cannot add more than
  /// the stack limit at once, so keep it consistent with the max stack limit of inventory slot.
  /// </summary>
  public int AddItemToChest(Item_Data item, int amount = 1) // change amount later
  {
    // Stack as much as you can
    int remaining = amount;
    foreach (var slot in itemsInChest)
    {
      if (remaining <= 0)
        break;

      if (slot.CanStack(item))
      {
        int added = slot.AddToExistingStack(item, remaining);
        remaining -= added;
      }
    }

    // Use empty slots for leftovers or if not stackable
    foreach (var slot in itemsInChest)
    {
      if (remaining <= 0)
        break;

      if (slot.IsEmpty())
      {
        int added = slot.StartNewStack(item, remaining);
        remaining -= added;
      }
    }

    if (remaining > 0)
      Debug.LogWarning($"[Chest]: Not enough space! {remaining} {item.name} could not be added.");

    return amount - remaining;
  }

  /// <summary>
  /// Attempts to remove 'amount' of 'item' from the chest.
  /// Returns the actual number removed.
  /// </summary>
  public int RemoveItemFromChest(Chest_Item slot, int amount = 1)
  {
    if (slot == null || slot.IsEmpty())
      return 0;

    int removed = Mathf.Min(slot.GetAmount(), amount);
    // slot.SetAmount(slot.GetAmount() - removed);

    return removed;
  }
  
  /// <summary>
  /// Get the chest save data for saving.
  /// </summary>
  public ChestSaveData GetChestSaveData()
  {
    ChestSaveData saveData = new ChestSaveData();
    foreach (Chest_Item slot in itemsInChest)
    {
      ChestItemData data = slot.ToData();
      if (data != null)
        saveData.itemsInChest.Add(data);
    }
    Debug.Log("[Chest] Generated chest save data.");
    Debug.Log($"[Chest] Saved {saveData.itemsInChest.Count} items in chest.");
    return saveData;
  }

  /// <summary>
  /// Load the chest from saved data.
  /// </summary>
  public void LoadChestData(ChestSaveData saveData)
  {
    // Clear all slots first
    foreach (Chest_Item slot in itemsInChest)
      slot.ClearItem();

    if (saveData == null || saveData.itemsInChest == null)
      return;

    for (int i = 0; i < saveData.itemsInChest.Count && i < itemsInChest.Count; i++)
    {
      if (saveData.itemsInChest[i] != null)
        Debug.Log($"[Chest] Loading item {i}: Category {saveData.itemsInChest[i].category}, DishType {saveData.itemsInChest[i].dishType}, Amount {saveData.itemsInChest[i].amount}");
      itemsInChest[i].FromData(saveData.itemsInChest[i]);
    }
    Debug.Log("[Chest] Loaded chest data from save.");
    Debug.Log($"[Chest] Loaded {saveData.itemsInChest.Count} items into chest.");
  }
}
