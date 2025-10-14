using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Grimoire;
using TMPro;

public class Shop : MonoBehaviour
{
  private bool isPlayerInRange = false;
  private bool shopOpen = false; 
  private bool firstOpen;
  Player_Progress playerProgress;
  [SerializeField] private GameObject shopUI;
  [SerializeField] private GameObject grid;
  [SerializeField] private GameObject shopItemPrefab;
  [SerializeField] private TextMeshProUGUI shopkeeperText; // Shopkeeper's "dialogue" text
  [SerializeField] public List<Shop_Item> items; // assign 6 slots in inspector
  [SerializeField] public Shop_Database shopDatabase;

  [Header("Shopkeeper Text Reactions")]
  [SerializeField] private string firstOpenText; // first open of the day
  [SerializeField] private string otherOpenText; // text for 2nd time and onwards open
  [SerializeField] private string buySuccessText; // text for when buy is successful
  [SerializeField] private string inventoryFullText; // text for when inventory full if buying that amount
  [SerializeField] private string notEnoughMoneyText; // text for when player doesn't have enough money

  [Header("Player Input Info")]
  private Player_Controller player;
  private InputAction interactAction;
  private InputAction closeAction;
  private PlayerInput playerInput;
  private bool interactPressed = false;

  private void Awake()
  {
    if (shopDatabase == null)
    {
      Debug.LogError("[Shop]: Shop database not set in inspector!");
      return;
    }

    if (Game_Manager.Instance == null)
    {
      Debug.LogWarning("[Shop]: Game_Manager instance is null!");
      return;
    }

    playerProgress = Game_Manager.Instance.playerProgress;
    playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
    if (playerInput != null)
    {
      interactAction = playerInput.actions["Interact"]; // From Player Input Map
      closeAction = playerInput.actions.FindAction("CloseInteract", true); // From UI Input Map
    }

    if (interactAction != null)
      interactAction.performed += InteractPressed;

    if (closeAction != null)
      closeAction.performed += InteractPressed;
    
    CloseShopUI();
    CreateShopItemCards();
    firstOpen = true;
  }

  private void Update()
  {
    if (!isPlayerInRange || !Game_Manager.Instance.UIManager.CanProcessInput())
      return;
      
    // Only process input if player is inside trigger and interact pressed once
    if (interactAction.WasPerformedThisFrame() || closeAction.WasPerformedThisFrame())
    {
      interactPressed = false;
      Debug.Log("[Shop]: Player interacted with shop.");

      if (player != null)
      {
        if (shopOpen)
          CloseShopUI();
        else if (!Game_Manager.Instance.UIManager.pauseMenuOn)
          OpenShopUI();
      }
    }
  }

  private void OnDestroy()
  {
    if (interactAction != null)
      interactAction.performed -= InteractPressed;

    if (closeAction != null)
      closeAction.performed -= InteractPressed;
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

  private void InteractPressed(InputAction.CallbackContext context)
  {
    if (isPlayerInRange)
      interactPressed = true;
    else
      interactPressed = false;
  }

  private void OpenShopUI()
  {
    shopUI.SetActive(true);
    shopOpen = true;
    Game_Manager.Instance.UIManager.OpenUI();

    if (!firstOpen)
      shopkeeperText.text = otherOpenText;
  }

  private void CloseShopUI()
  {
    shopUI.SetActive(false);
    if (shopOpen)
      Game_Manager.Instance.UIManager.CloseUI();
    shopOpen = false;

    if (firstOpen)
      firstOpen = false;
  }

  /// <summary>
  /// Sets all the data for each item card in the shop from shop database. Also sets firstOpenText.
  /// </summary>
  private void CreateShopItemCards()
  {
    for (int i = 0; i < items.Count; i++)
    {
      Shop_Item item = items[i];
      if (i < shopDatabase.allItems.Count) // Show item only if there are still item cards in grid
        item.SetItem(shopDatabase.allItems[i], this);
      else // Don't show empty cards if shopDatabase doesn't hold enough to fill them all
        item.ClearItem();
    }

    shopkeeperText.text = firstOpenText;
  }

  /// <summary>
  /// Calculates total cost of buying amount number of item and responds accordingly.
  /// Returns true if successfully bought and added all amount number of item into inventory. 
  /// Returns false otherwise (inventory is full or not enough money).
  /// </summary>
  private bool BuyItem(Ingredient_Data item, int amount)
  {
    float totalCost = item.price * amount;
    // Debug.Log("[Shop]: Player has " + playerProgress.GetMoneyAmount() + " money left.");
    // Debug.Log("[Shop]: Player is trying to spend " + totalCost);

    if (totalCost <= playerProgress.GetMoneyAmount())
    {
      int amountActuallyAdded = Ingredient_Inventory.Instance.AddResources(item.ingredientType, amount);
      if (amount == amountActuallyAdded)
      {
        playerProgress.SubtractMoney(totalCost);
        shopkeeperText.text = buySuccessText;
        return true;
      }

      // Inventory didn't add enough (most likely didn't have space to add all)
      shopkeeperText.text = inventoryFullText;
      Ingredient_Inventory.Instance.RemoveResources(item.ingredientType, amountActuallyAdded); // remove from inventory what was already added
      return false;
    }

    // not enough money
    shopkeeperText.text = notEnoughMoneyText;
    return false;
  }

  #region Shop_Item Class
  [System.Serializable]
  public class Shop_Item
  {
    [SerializeField] private GameObject background; // the button's background object, used to turn off when no item
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button addButton;
    [SerializeField] private TextMeshProUGUI priceText;
    Shop shop;
    private Ingredient_Data currentItem;
    private int currentAmount = 0;

    public void SetItem(Ingredient_Data item, Shop parent)
    {
      shop = parent;
      background.SetActive(true);
      currentItem = item;
      nameText.text = item.Name;
      itemImage.sprite = item.Image;
      itemImage.preserveAspect = true;
      amountText.text = "0";
      priceText.text = item.price.ToString();

      // Set the button clicks
      buyButton.onClick.RemoveAllListeners();
      buyButton.onClick.AddListener(BuyButton);

      minusButton.onClick.RemoveAllListeners();
      minusButton.onClick.AddListener(SubtractAmountButton);

      addButton.onClick.RemoveAllListeners();
      addButton.onClick.AddListener(AddAmountButton);
    }

    public void ClearItem()
    {
      currentItem = null;
      buyButton.onClick.RemoveAllListeners();
      minusButton.onClick.RemoveAllListeners();
      addButton.onClick.RemoveAllListeners();
      currentAmount = 0;
      background.SetActive(false);
    }

    public void AddAmountButton()
    {
      Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.clickSFX, 0.8f);
      currentAmount++;
      amountText.text = currentAmount.ToString();
    }

    public void SubtractAmountButton()
    {
      Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.clickSFX, 0.8f);
      if (currentAmount != 0) // if 0, then do nothing
      {
        currentAmount--;
        amountText.text = currentAmount.ToString();
      }
    }

    public void BuyButton()
    {
      Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.clickSFX, 0.8f);
      if (currentAmount == 0) // Ignore buy button press if buying 0 amount
        return;

      if(shop.BuyItem(currentItem, currentAmount))
      {
        currentAmount = 0;
        amountText.text = currentAmount.ToString();
      }
    }
  }
  #endregion
}
