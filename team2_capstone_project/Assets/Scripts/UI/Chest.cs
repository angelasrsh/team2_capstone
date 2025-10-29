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
    public static event System.Action<bool> OnChestOpenChanged;
    private bool isPlayerInRange = false;
    public bool chestOpen = false; 
    [SerializeField] private GameObject ChestUI;
    [SerializeField] private GameObject grid;
    [SerializeField] public RectTransform redZone; // only here so that drag_all can easily find redZone even when it's inactive
    [SerializeField] public List<Chest_Item> itemsInChest; // assign 18 slots in inspector

    
    [Header("Player Input Info")]
    private Player_Controller player;
    private InputAction interactAction, closeAction;
    private PlayerInput playerInput;

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
        CreateChestSlots();
        CloseChestUI();
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
    }

    private void CloseChestUI()
    {
        ChestUI.SetActive(false);
        if (chestOpen)
            Game_Manager.Instance.UIManager.CloseUI();
        chestOpen = false;

        // Notify draggables to disable drag when trash closed
        OnChestOpenChanged?.Invoke(chestOpen);
    }


    /// <summary>
    /// Sets the data for each item slot in the trash. These slots should have empty images, empty amounts.
    /// </summary>
    private void CreateChestSlots()
    {
        foreach (Transform slot in grid.transform)
        {
            Chest_Item newChestItem = new Chest_Item(slot);
            itemsInChest.Add(newChestItem);
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

    #region Chest_Item Class
    [System.Serializable]
  public class Chest_Item
  {
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI amountText;
    // [SerializeField] private Toggle toggle;
    private Item_Data currentItem; // object since it can be dish or ingredient
    private int currentAmount;
    private const int MAX_STACK = 10; // stack limit for ingredients

    public Chest_Item(Transform slot)
    {
      itemImage = slot.Find("Image").GetComponent<Image>();
      amountText = slot.Find("Amount").GetComponent<TextMeshProUGUI>();
      ClearItem();
    }

    public bool IsEmpty()
    {
      return currentItem == null;
    }

    public Item_Data GetItem()
    {
      return currentItem;
    }

    public int GetAmount()
    {
      return currentAmount;
    }

    public bool CanStack(Item_Data newItem)
    {
      return newItem is Ingredient_Data ingredient && currentItem == ingredient && currentAmount < MAX_STACK;
    }

    /// <summary>
    /// Add to this slot's existing stack if possible, and return how many items were actually added.
    /// </summary>
    public int AddToExistingStack(Item_Data newItem, int amount)
    {
      if (!CanStack(newItem))
        return 0;

      int spaceLeft = MAX_STACK - currentAmount;
      int added = Mathf.Min(spaceLeft, amount);
      currentAmount += added;
      UpdateVisuals();
      return added;
    }

    /// <summary>
    /// Start a new stack in this empty slot. Returns how many items were actually added.
    /// </summary>
    public int StartNewStack(Item_Data newItem, int amount)
    {
      if (!IsEmpty())
        return 0;

      currentItem = newItem;
      int added = 1;
      if (newItem is Ingredient_Data)
        added = Mathf.Min(amount, MAX_STACK);

      currentAmount = added;
      UpdateVisuals();
      return added;
    }

    public void ClearItem()
    {
      // if (currentItem == null && currentAmount == 0)
      //   return;

      currentItem = null;
      currentAmount = 0;
      currentItem = null;
      amountText.gameObject.SetActive(false);
      itemImage.gameObject.SetActive(false);
    }

    private void UpdateVisuals()
    {
      if (currentItem != null)
      {
        itemImage.sprite = currentItem.Image;
        itemImage.preserveAspect = true;
        itemImage.gameObject.SetActive(true);

        bool isIngredient = currentItem is Ingredient_Data;
        if (isIngredient)
        {
          amountText.text = currentAmount.ToString();
          amountText.gameObject.SetActive(true);
        }
        else
          amountText.gameObject.SetActive(false);
      }
      else
        ClearItem();
    }
  }
  #endregion
}
