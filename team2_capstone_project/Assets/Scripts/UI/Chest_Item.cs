using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Chest_Item: MonoBehaviour, IPointerDownHandler
{
  [SerializeField] private Image itemImage;
  [SerializeField] private TextMeshProUGUI amountText;
  private Item_Data currentItem; // Item_Data since it can be dish or ingredient
  private int currentAmount;
  private const int MAX_STACK = 10; // stack limit for ingredients

  private float lastTapTime = 0f;
  private const float doubleTapThreshold = 0.25f;
  private static Chest chest; // static because we only have one chest (change later if multiple chests)

  public void Setup(Transform slot)
  {
    itemImage = slot.Find("Image").GetComponent<Image>();
    amountText = slot.Find("Amount").GetComponent<TextMeshProUGUI>();
    ClearItem();
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    float timeSinceLastTap = Time.time - lastTapTime;

    if (timeSinceLastTap <= doubleTapThreshold)
    {
        TakeOneItem();
        lastTapTime = 0f; // reset so triple taps won't trigger multiple removals
    }
    else
    {
        lastTapTime = Time.time; // first tap, wait for second
    }
  }

  private void TakeOneItem()
  {
    if (currentItem == null)
      return;

    if (Chest.Instance == null)
      return;

    int removed = Chest.Instance.RemoveItemFromChest(this, 1);

    if (removed > 0)
    {
      // Add to inventory
      if (currentItem is Ingredient_Data ingredient)
        Ingredient_Inventory.Instance.AddResources(ingredient.ingredientType, removed);
      else if (currentItem is Dish_Data dish)
        Dish_Tool_Inventory.Instance.AddResources(dish, removed);

      currentAmount -= removed;
      UpdateVisuals();
    }
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

  public void SetAmount(int newAmount)
  {
    currentAmount = Mathf.Max(0, newAmount);
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
    if (currentItem != null && currentAmount > 0)
    {
      itemImage.sprite = currentItem.Image;
      itemImage.preserveAspect = true;
      itemImage.gameObject.SetActive(true);

      if (currentItem is Ingredient_Data)
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

  public ChestItemData ToData()
  {
    if (currentItem == null)
      return null;

    ChestItemData data = new ChestItemData { amount = currentAmount };
    if (currentItem is Dish_Data dish)
    {
        data.category = ItemCategory.Dish;
        data.dishType = dish.dishType;
    }
    else if (currentItem is Ingredient_Data ingredient)
    {
        data.category = ItemCategory.Ingredient;
        data.ingredientType = ingredient.ingredientType;
    }

    return data;
  }
  
  public void FromData(ChestItemData data)
  {
    if (data == null)
    {
      ClearItem();
      return;
    }

    switch (data.category)
    {
      case ItemCategory.Dish:
            currentItem = Game_Manager.Instance.dishDatabase.GetDish(data.dishType) 
                          ?? Game_Manager.Instance.dishDatabase.GetBadDish();
            break;
        case ItemCategory.Ingredient:
            currentItem = Game_Manager.Instance.ingredientDatabase.GetIngredient(data.ingredientType);
            break;
    }

    currentAmount = data.amount;
    Debug.Log($"[Chest_Item] Loaded item {currentItem.name} x{currentAmount} from data.");
    UpdateVisuals();
  }
}