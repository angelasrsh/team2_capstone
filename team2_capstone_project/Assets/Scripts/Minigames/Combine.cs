using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Combine : MonoBehaviour
{
  private List<Ingredient_Data> ingredientsOnTable;
  [SerializeField] private List<RectTransform> redZones;
  [SerializeField] private List<Image> ingredientImages;
  [SerializeField] private GameObject errorText;
  private Dictionary<IngredientType, int> howManyOfEach;

  [Header("References for switching minigame sections")]
  [SerializeField] private GameObject combineCanvas; // reference to the combine table's canvas
  [SerializeField] private GameObject mixCanvas; // reference to the mixing section's canvas
  [SerializeField] private GameObject inventoryCanvas; // reference to the inventory canvas
  [SerializeField] private GameObject dishInventoryCanvas; // reference to the dish inventory canvas
  [SerializeField] private GameObject backButton; // reference to the back button
  [SerializeField] private GameObject clearButton; // reference to the clear button
  [SerializeField] private GameObject combineButton; // reference to the combine button
  // [SerializeField] private Image ingredientInPanAnim; // reference to ingredient in pan under pan flip animation
  // [SerializeField] private GameObject regularPan; // reference to the regular pan
  // [SerializeField] private GameObject draggablePan; // reference to the pan that can be dragged around

  private void Awake()
  {
    ingredientsOnTable = new List<Ingredient_Data>();
    howManyOfEach = new Dictionary<IngredientType, int>();
    errorText.SetActive(false);
    // audio = Audio_Manager.instance;
  }

  private void Start()
  {
    ResetToCombineTable();

    GameObject grid = GameObject.Find("Ingredient_Grid");
    if (redZones == null || redZones.Count != 6)
    {
      // Find the redzones
      redZones.Clear();
      redZones.Add(grid.transform.GetChild(0).GetComponent<RectTransform>());
      redZones.Add(grid.transform.GetChild(1).GetComponent<RectTransform>());
      redZones.Add(grid.transform.GetChild(2).GetComponent<RectTransform>());
      redZones.Add(grid.transform.GetChild(3).GetComponent<RectTransform>());
      redZones.Add(grid.transform.GetChild(4).GetComponent<RectTransform>());
      redZones.Add(grid.transform.GetChild(5).GetComponent<RectTransform>());
    }
    if (ingredientImages == null || ingredientImages.Count != 6)
    {
      // Find the images
      ingredientImages.Clear();
      ingredientImages.Add(redZones[0].Find("Ingredient").GetComponent<Image>());
      ingredientImages.Add(redZones[1].Find("Ingredient").GetComponent<Image>());
      ingredientImages.Add(redZones[2].Find("Ingredient").GetComponent<Image>());
      ingredientImages.Add(redZones[3].Find("Ingredient").GetComponent<Image>());
      ingredientImages.Add(redZones[4].Find("Ingredient").GetComponent<Image>());
      ingredientImages.Add(redZones[5].Find("Ingredient").GetComponent<Image>());
    }
    for (int i = 0; i < redZones.Count; i++)
    {
      ingredientsOnTable.Add(null);
      ingredientImages[i].gameObject.SetActive(false);
    }
  }

  private void ResetToCombineTable()
  {
    combineCanvas.SetActive(true);
    mixCanvas.SetActive(false);
    backButton.SetActive(true);
    clearButton.SetActive(true);
    combineButton.SetActive(true);
    inventoryCanvas.SetActive(true);
    dishInventoryCanvas.SetActive(true);
  }

  private void ChangeToMixMinigame()
  {
    combineCanvas.SetActive(false);
    mixCanvas.SetActive(true);
    backButton.SetActive(false);
    clearButton.SetActive(false);
    combineButton.SetActive(false);
    inventoryCanvas.SetActive(false);
    dishInventoryCanvas.SetActive(false);
  }

  /// <summary>
  /// Checks if you can make a valid dish. If not, error message shows up.
  /// </summary>
  public void DoCombine()
  {
    // Collect all ingredients currently on the table
    List<Ingredient_Data> currentIngredients = new List<Ingredient_Data>();
    foreach (var ing in ingredientsOnTable)
    {
      if (ing != null)
        currentIngredients.Add(ing);
    }

    // If no ingredients are on the table, show error
    if (currentIngredients.Count == 0)
    {
      errorText.SetActive(true);
      Invoke(nameof(HideErrorText), 3f);
      return;
    }

    Ingredient_Data firstIngredient = currentIngredients[0];
    Dish_Data dishMade = null;
    Ingredient_Data ingredientMade = null;

    // Try to find a matching dish recipe
    foreach (Dish_Data dish in firstIngredient.usedInDishes)
    {
      if (dish.recipe != Recipe.Combine && dish.recipe != Recipe.Mix)
        continue;

      if (RecipeMatchesDish(dish, currentIngredients))
      {
        dishMade = dish;
        break;
      }
    }

    // If no dish found, check for ingredient recipes
    if (dishMade == null && firstIngredient.makesIngredient != null)
    {
      foreach (Ingredient_Requirement ing in firstIngredient.makesIngredient)
      {
        if (ing.method != Recipe.Combine && ing.method != Recipe.Mix)
          continue;

        Ingredient_Data potentialIng = ing.ingredient;
        if (potentialIng == null || potentialIng.ingredientsNeeded == null)
          continue;

        if (RecipeMatchesIngredient(potentialIng, currentIngredients))
        {
          ingredientMade = potentialIng;
          break;
        }
      }
    }

    if (ingredientMade != null)
    {
      // Debug.Log("[Combine]: Ingredient made - " + ingredientMade.Name);
      Ingredient_Inventory.Instance.AddResources(ingredientMade.ingredientType, 1);
    }
    else if (dishMade != null)
    {
      // Debug.Log("[Combine]: Dish made - " + dishMade.Name);
      if (dishMade.recipe == Recipe.Mix)
      {
          StartMixMinigame(dishMade);
          return;
      }
      else // Combine
      {
          Dish_Tool_Inventory.Instance.AddResources(dishMade, 1);
      }
    }
    else
    {
      // Debug.Log("[Combine]: No valid recipe found. Showing error text.");
      errorText.SetActive(true);
      Invoke(nameof(HideErrorText), 3f);
      return;
    }

    ClearTable(false); // Dont return ingredients to inventory
  }

  private bool RecipeMatchesDish(Dish_Data dish, List<Ingredient_Data> currentIngredients)
  {
    if (dish.ingredientQuantities.Count != currentIngredients.Count)
      return false;

    foreach (Ingredient_Requirement req in dish.ingredientQuantities)
    {
      if (!howManyOfEach.ContainsKey(req.ingredient.ingredientType) || howManyOfEach[req.ingredient.ingredientType] != req.amountRequired)
        return false;
    }

    return true;
  }

  private bool RecipeMatchesIngredient(Ingredient_Data candidate, List<Ingredient_Data> currentIngredients)
  {
    if (candidate.ingredientsNeeded.Count != currentIngredients.Count)
      return false;

    foreach (Ingredient_Requirement req in candidate.ingredientsNeeded)
    {
      if (!howManyOfEach.ContainsKey(req.ingredient.ingredientType) || howManyOfEach[req.ingredient.ingredientType] != req.amountRequired)
        return false;
    }

    return true;
  }

  private void StartMixMinigame(Dish_Data dish)
  {
    Debug.Log("[Combine]: Starting Mix Minigame for " + dish.Name);
    ClearTable(false); // remove ingredients but don't give them back
    ChangeToMixMinigame();

    // Call the mix_minigame method
    Mix_Minigame.Instance.StartMinigame(
      dish.mixDuration,
      dish.targetClicksPerSecond,
      dish.tolerance,
      () =>
        {
          // success callback
          Dish_Tool_Inventory.Instance.AddResources(dish, 1);
          ResetToCombineTable();
        },
      (string failReason) =>
        {
          // failure callback
          errorText.SetActive(true);
          errorText.GetComponent<TextMeshProUGUI>().text = failReason;
          Invoke(nameof(HideErrorText), 3f);
          ResetToCombineTable();
        }
    );
  }

  /// <summary>
  /// Add ingredient to combine table if possible and update sprite of the zone accordingly.
  /// </summary>
  public bool AddToTable(Ingredient_Data ing, int zone)
  {
    if (ingredientsOnTable[zone] != null) // if that zone is occupied, do not let it add
      return false;

    Debug.Log("[Combine]: Adding to table");
    ingredientsOnTable[zone] = ing;
    ingredientImages[zone].sprite = ing.Image;
    ingredientImages[zone].preserveAspect = true;
    ingredientImages[zone].gameObject.SetActive(true);

    if (howManyOfEach.ContainsKey(ing.ingredientType))
        howManyOfEach[ing.ingredientType]++;
    else
        howManyOfEach[ing.ingredientType] = 1;

    Game_Events_Manager.Instance.CombineAddToTable(ing, zone);
    return true;
  }

  /// <summary>
  /// Clear table and return ingredients to inventory if there are any on the table.
  /// </summary>
  public void ClearTable(bool addToInventory)
  {
    for (int i = 0; i < ingredientsOnTable.Count; i++)
    {
      if (ingredientsOnTable[i] != null && addToInventory)
      {
        Ingredient_Inventory.Instance.AddResources(ingredientsOnTable[i].ingredientType, 1);
      }
      ingredientsOnTable[i] = null;
      howManyOfEach.Clear();

      ingredientImages[i].gameObject.SetActive(false);
    }
  }

  /// <summary>
  /// Used to check if the mouse while dragging an ingredient from the inventory is over 
  /// some red zone. Returns the slot number that overlapped.
  /// </summary>
  public int IsOverARedZone()
  {
    if (redZones == null || redZones.Count == 0)
      return -1;

    Vector2 mousePos = Input.mousePosition;
    for (int i = 0; i < redZones.Count; i++)
    {
      RectTransform zone = redZones[i];
      if (zone == null)
      {
        Debug.LogWarning("[Combine]: Redzone " + i + " is null. Most likely not set in inspector!");
        continue;
      }

      Vector3[] corners = new Vector3[4];
      zone.GetWorldCorners(corners);

      // Convert world corners to screen space
      Vector3 bl = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
      Vector3 tr = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

      Rect screenRect = new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);

      if (screenRect.Contains(mousePos))
        return i;
    }

    return -1; // No red zone under the mouse
  }

  private void HideErrorText()
  {
    errorText?.SetActive(false);
  }
}
