using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Grimoire;
using TMPro;

public class Pan : MonoBehaviour
{
  private Ingredient_Data ingredientInPan;
  private Ingredient_Data cookedIngredientData;
  private CookedState firstCookedState = CookedState.Raw;
  private CookedState secondCookedState = CookedState.Raw;
  [SerializeField] private GameObject errorText;
  [SerializeField] private Image ingrInPanImage; // reference to the pan's Image component
  [SerializeField] private GameObject inventoryCanvas; // reference to the inventory canvas
  [SerializeField] private GameObject dishInventoryCanvas; // reference to the dish inventory canvas

  [Header("Slider")]
  private Slider sliderComponent;
  [SerializeField] private GameObject slider; // reference to the cooking slider
  // [SerializeField] private Image rawZone;
  // [SerializeField] private Image cookedZone;
  // [SerializeField] private Image burntZone;
  [SerializeField] private RectTransform rawRect;
  [SerializeField] private RectTransform almostRect;
  [SerializeField] private RectTransform cookedRect;
  [SerializeField] private RectTransform overcookedRect;
  [SerializeField] private RectTransform burntRect;
  // [Range(0, 1)] public float rawEnd = 0.4f;
  // [Range(0, 1)] public float almostEnd = 0.6f;
  // [Range(0, 1)] public float cookedEnd = 0.7f;
  // [Range(0, 1)] public float overcookedEnd = 0.8f;
  // burnt zone will fill the rest (overcookedEnd to 1)

  public enum CookedState
  {
    Raw,
    Cooked,
    Burnt
  }

  public void Start()
  {
    sliderComponent = slider.GetComponent<Slider>();
    if (sliderComponent == null)
      Debug.LogError("[Pan]: No Slider component found on Slider GameObject!");
      ResetAll();
  }

  /// <summary>
  /// Resets frying pan to initial state.
  /// </summary>
  public void ResetAll()
  {
    inventoryCanvas.SetActive(true);
    dishInventoryCanvas.SetActive(true);
    ingredientInPan = null;
    cookedIngredientData = null;
    firstCookedState = CookedState.Raw;
    secondCookedState = CookedState.Raw;
    ingrInPanImage.gameObject.SetActive(false);
    sliderComponent.value = 0;
    slider.SetActive(false);
  }

  /// <summary>
  /// Call this to add the ingredient to the pan. This should be called in Drag_All when 
  /// an ingredient is dropped into the pan's red zone.
  /// </summary>
  public bool AddToPan(Ingredient_Data ingredient)
  {
    // if (ingredientInPan != null)
    // {
    //   Debug.Log("[Pan]: Pan already has an ingredient. Cannot add another.");
    //   errorText.GetComponent<TMPro.TMP_Text>().text = "Pan already has an ingredient! Cannot add another!";
    //   errorText.SetActive(true);
    //   Invoke(nameof(HideErrorText), 3);
    //   return false;
    // }

    // Check if ingredient can be fried
    bool canBeFried = false;
    foreach (var ingrMade in ingredient.makesIngredient)
    {
      if (ingrMade.method == Recipe.Fry)
      {
        cookedIngredientData = ingrMade.ingredient;
        canBeFried = true;
        break;
      }
    }

    if (!canBeFried)
    {
      Debug.Log("[Pan]: Ingredient " + ingredient.name + " cannot be cooked.");
      errorText.GetComponent<TMPro.TMP_Text>().text = ingredient.Name + " cannot be cooked!";
      errorText.SetActive(true);
      Invoke(nameof(HideErrorText), 3);
      return false;
    }

    // Debug.Log("[Pan]: Added " + ingredient.Name + " to pan.");
    ingrInPanImage.gameObject.SetActive(true);
    ingredientInPan = ingredient;
    if (ingredientInPan.CutIngredientImages.Length > 0)
    {
      ingrInPanImage.sprite = ingredientInPan.CutIngredientImages[0];
    }
    else
    {
      ingrInPanImage.sprite = ingredient.Image;
    }
    // ingrInPanImage.preserveAspect = true;

    // SFX
    // if (ingredient.Name == "Water")
    // {
    //     Audio_Manager.instance.AddWater();
    // }
    // else
    //   Audio_Manager.instance.AddOneIngredient();
    inventoryCanvas.SetActive(false);
    dishInventoryCanvas.SetActive(false);
    slider.SetActive(true);
    // sliderComponent.value = 0;

    UpdateSliderZones(ingredientInPan);
    return true;
  }

  /// <summary>
  /// Allows for each ingredient to have different raw/cooked/burnt zones.
  /// </summary>
  private void UpdateSliderZones(Ingredient_Data ingredient)
  {
    if (!slider)
      return;

    if (!rawRect || !almostRect || !cookedRect || !overcookedRect || !burntRect) return;

    // Default thresholds
    float rawEnd = 0.4f;
    float almostEnd = 0.6f;
    float cookedEnd = 0.7f;
    float overcookedEnd = 0.8f;

    // Use ingredient-specific thresholds if available
    if (ingredient.cookThresholds != null)
    {
      rawEnd = ingredient.cookThresholds.rawEnd;
      almostEnd = ingredient.cookThresholds.almostEnd;
      cookedEnd = ingredient.cookThresholds.cookedEnd;
      overcookedEnd = ingredient.cookThresholds.overcookedEnd;
      ingredient.cookThresholds.ClampValues();
    }

    // Raw zone
    rawRect.anchorMin = new Vector2(0f, 0f);
    rawRect.anchorMax = new Vector2(rawEnd, 1f);
    rawRect.offsetMin = rawRect.offsetMax = Vector2.zero;

    // Almost cooked
    almostRect.anchorMin = new Vector2(rawEnd, 0f);
    almostRect.anchorMax = new Vector2(almostEnd, 1f);
    almostRect.offsetMin = almostRect.offsetMax = Vector2.zero;

    // Cooked
    cookedRect.anchorMin = new Vector2(almostEnd, 0f);
    cookedRect.anchorMax = new Vector2(cookedEnd, 1f);
    cookedRect.offsetMin = cookedRect.offsetMax = Vector2.zero;

    // Overcooked
    overcookedRect.anchorMin = new Vector2(cookedEnd, 0f);
    overcookedRect.anchorMax = new Vector2(overcookedEnd, 1f);
    overcookedRect.offsetMin = overcookedRect.offsetMax = Vector2.zero;

    // Burnt
    burntRect.anchorMin = new Vector2(overcookedEnd, 0f);
    burntRect.anchorMax = new Vector2(1f, 1f);
    burntRect.offsetMin = burntRect.offsetMax = Vector2.zero;

    // Optional: assign colors (Cooking Mama style)
    // rawRect.GetComponent<Image>().color = new Color32(255, 76, 76, 255);       // Red
    // almostRect.GetComponent<Image>().color = new Color32(255, 213, 79, 255);   // Yellow
    // cookedRect.GetComponent<Image>().color = new Color32(102, 255, 102, 255);  // Green
    // overcookedRect.GetComponent<Image>().color = new Color32(255, 112, 67, 255);// Orange
    // burntRect.GetComponent<Image>().color = new Color32(62, 39, 35, 255);
  }

  public bool IsEmpty()
  {
    return ingredientInPan == null;
  }

  public void HideErrorText()
  {
    errorText?.SetActive(false);
  }
}
