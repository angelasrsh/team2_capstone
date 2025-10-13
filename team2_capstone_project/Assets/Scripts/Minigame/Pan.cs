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
  [SerializeField] private GameObject flipAnimation; // reference to the flipping animation
  [SerializeField] private GameObject regularPan; // reference to the regular pan
  [SerializeField] private GameObject draggablePan; // reference to the pan that can be dragged around
  [SerializeField] private Ingredient_Data burntIngredient; // generic burnt ingredient to use
  private Pan_Controller panController;
  private Color originalColor;

  [Header("Slider")]
  private Slider sliderComponent;
  [SerializeField] private GameObject slider; // reference to the cooking slider
  [SerializeField] private RectTransform rawRect;
  [SerializeField] private RectTransform almostRect;
  [SerializeField] private RectTransform cookedRect;
  [SerializeField] private RectTransform overcookedRect;
  [SerializeField] private RectTransform burntRect;
  bool sliderActive = false;
  bool afterFirstCook = false;

  public enum CookedState
  {
    Raw,
    Almost,
    Cooked,
    Overcooked,
    Burnt
  }

  public void Start()
  {
    sliderComponent = slider.GetComponent<Slider>();
    if (sliderComponent == null)
      Debug.LogError("[Pan]: No Slider component found on Slider GameObject!");
    // sliderComponent.interactable = false;
    originalColor = ingrInPanImage.color;
    ResetAll();
    panController = draggablePan.GetComponent<Pan_Controller>();
    if (panController == null)
      Debug.LogError("[Pan]: No Pan_Controller component found on Draggable Pan GameObject!");
  }

  private void Update()
  {
    if (!sliderActive || ingredientInPan == null)
      return;

    float speedMultiplier = 1f + sliderComponent.value * 1.8f; // Speed increases slightly as slider moves right

    // Move slider value
    sliderComponent.value += speedMultiplier * Time.deltaTime * 0.5f; // Base speed is 0.5f
    if (sliderComponent.value >= 1f)
    {
      sliderComponent.value = 1f;
      sliderActive = false;
      Debug.Log("[Pan]: Slider reached the end without stopping. Ingredient is burnt.");
      if (!afterFirstCook)
      {
        firstCookedState = CookedState.Burnt;
        StartCoroutine(ShowZoneThenNext(firstCookedState));
      }
      else
      {
        secondCookedState = CookedState.Burnt;
        StartCoroutine(ShowZoneThenNext(secondCookedState));
      }
      return;
    }

    // Stop cooking on mouse click
    if (Input.GetMouseButtonDown(0))
    {
      sliderActive = false;
      float val = sliderComponent.value;

      // Determine cooked state based on zones
      if (val <= rawRect.anchorMax.x)
      {
        // keep the below comment in case we want to make raw blobs later
        // if (!afterFirstCook)
        //   firstCookedState = CookedState.Raw;
        // else
        //   secondCookedState = CookedState.Raw;
        errorText.GetComponent<TMPro.TMP_Text>().text = "Ingredient is still Raw! Try again.";
        errorText.SetActive(true);
        Invoke(nameof(HideErrorText), 3);
        Invoke(nameof(StartSlider), 1.5f); // Restart slider if stopped in raw zone
        return;
        // Debug.Log("[Pan]: Ingredient is still Raw.");
      }
      else if (val <= almostRect.anchorMax.x)
      {
        if (!afterFirstCook)
          firstCookedState = CookedState.Almost; // Almost cooked treated as Cooked for simplicity (for now)
        else
          secondCookedState = CookedState.Almost;
        // Debug.Log("[Pan]: Ingredient is Almost Cooked.");
      }
      else if (val <= cookedRect.anchorMax.x)
      {
        if (!afterFirstCook)
          firstCookedState = CookedState.Cooked;
        else
          secondCookedState = CookedState.Cooked;
        // Debug.Log("[Pan]: Ingredient is Perfectly Cooked!");
      }
      else if (val <= overcookedRect.anchorMax.x)
      {
        if (!afterFirstCook)
          firstCookedState = CookedState.Overcooked; // Overcooked treated as Cooked for simplicity (for now)
        else
          secondCookedState = CookedState.Overcooked;
        // Debug.Log("[Pan]: Ingredient is Overcooked.");
      }
      else
      {
        if (!afterFirstCook)
          firstCookedState = CookedState.Burnt;
        else
          secondCookedState = CookedState.Burnt;
        // Debug.Log("[Pan]: Ingredient is Burnt.");
      }

      if (!afterFirstCook)
        StartCoroutine(ShowZoneThenNext(firstCookedState));
      else
        StartCoroutine(ShowZoneThenNext(secondCookedState));
    }
  }

  /// <summary>
  /// Highlights the given zone for a short time, then either starts the animation before the ingredeient fall section
  /// or completes cooking.
  /// </summary>
  private IEnumerator ShowZoneThenNext(CookedState state)
  {
    HighlightZone(state);
    yield return new WaitForSeconds(2f); // actually ends up waiting 0.8f secs because of the flashing animation

    if (!afterFirstCook)
    {
      afterFirstCook = true;
      StartAnimation();
      sliderComponent.value = 0f;
    }
    else
      CompleteCooking();
  }

  /// <summary>
  /// Highlights the given zone to indicate where the player stopped the slider using FlashZone.
  /// </summary>
  private void HighlightZone(CookedState state)
  {
    RectTransform target = null;

    switch (state)
    {
      case CookedState.Raw:
        target = rawRect;
        break;
      case CookedState.Almost:
        target = almostRect;
        break;
      case CookedState.Cooked:
        target = cookedRect;
        break;
      case CookedState.Overcooked:
        target = overcookedRect;
        break;
      case CookedState.Burnt:
        target = burntRect;
        break;
    }

    if (target != null)
      StartCoroutine(FlashZone(target));
  }

  /// <summary>
  /// Flashes the given zone to indicate where the player stopped the slider.
  /// </summary>
  private IEnumerator FlashZone(RectTransform zone)
  {
    Image zoneImage = zone.GetComponent<Image>();

    if (zoneImage == null)
      yield break;

    Color origColor = zoneImage.color;
    Color flashColor = Color.white; // Flash to white

    int flashes = 3; // how many flashes
    float flashSpeed = 0.2f; // duration of one flash

    for (int i = 0; i < flashes; i++)
    {
      // flash in
      for (float t = 0; t < 1f; t += Time.deltaTime / flashSpeed)
      {
        zoneImage.color = Color.Lerp(origColor, flashColor, t);
        yield return null;
      }
      // flash out
      for (float t = 0; t < 1f; t += Time.deltaTime / flashSpeed)
      {
        zoneImage.color = Color.Lerp(flashColor, origColor, t);
        yield return null;
      }
    }

    zoneImage.color = origColor; // reset to original color
  }

  /// <summary>
  /// Resets frying pan to initial state.
  /// </summary>
  public void ResetAll()
  {
    inventoryCanvas.SetActive(true);
    dishInventoryCanvas.SetActive(true);
    regularPan.SetActive(true);
    flipAnimation.SetActive(false);
    ingredientInPan = null;
    cookedIngredientData = null;
    firstCookedState = CookedState.Raw;
    secondCookedState = CookedState.Raw;
    ingrInPanImage.gameObject.SetActive(false);
    sliderComponent.value = 0;
    slider.SetActive(false);
    sliderActive = false;
    afterFirstCook = false;
    ingrInPanImage.color = originalColor;
    SetImageAlpha(ingrInPanImage, 1f);
  }

  /// <summary>
  /// Call this to add the ingredient to the pan. This should be called in Drag_All when 
  /// an ingredient is dropped into the pan's red zone.
  /// </summary>
  public bool AddToPan(Ingredient_Data ingredient)
  {
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
    UpdateSliderZones(ingredientInPan);
    panController.SetFallingIngredient(ingredientInPan);
    return true;
  }

  /// <summary>
  /// Allows for each ingredient to have different raw/cooked/burnt zones.
  /// </summary>
  private void UpdateSliderZones(Ingredient_Data ingredient)
  {
    if (!rawRect || !almostRect || !cookedRect || !overcookedRect || !burntRect)
      return;

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
  }

  /// <summary>
  /// Starts the flipping animation and after it ends, shows the draggable pan and starts the ingredient fall section.
  /// </summary>
  private void StartAnimation()
  {
    regularPan.SetActive(false);
    slider.SetActive(false);
    flipAnimation.SetActive(true);

    Animator anim = flipAnimation.GetComponent<Animator>();
    anim.SetTrigger("Flip");
    StartCoroutine(EndAnimation(anim));

    // Play sfx
    // Audio_Manager.instance.FinishCooking();
  }

  /// <summary>
  /// Waits for the flipping animation to finish before switching back to the draggable pan and starting
  /// the ingredient fall section.
  /// </summary>
  private IEnumerator EndAnimation(Animator animator)
  {
    // Wait for the animation to actually start
    yield return null;

    // Get current animation info
    AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

    // Wait for its duration and an extra pause
    yield return new WaitForSeconds(info.length + 1.3f);

    animator.Play("Idle");
    animator.Update(0f); // Force update to apply the idle state immediately
    flipAnimation.SetActive(false);

    yield return new WaitForSeconds(0.5f); // Small delay before showing pan again
    draggablePan.SetActive(true);

    yield return new WaitForSeconds(0.5f); // Small delay before starting ingredient fall section
    panController.StartIngredientFall();
  }

  /// <summary>
  /// Called after the second cooking phase is done to finalize the cooking process.
  /// </summary>
  private void CompleteCooking()
  {
    // Determine final cooked ingredient based on cooking states
    if (firstCookedState == CookedState.Cooked && secondCookedState == CookedState.Cooked)
      Ingredient_Inventory.Instance.AddResources(cookedIngredientData, 1);
    else if (firstCookedState == CookedState.Raw && secondCookedState == CookedState.Raw) // Shouldn't get to here (for now)
      Ingredient_Inventory.Instance.AddResources(ingredientInPan, 1);
    else if (firstCookedState == CookedState.Burnt || secondCookedState == CookedState.Burnt)
      Ingredient_Inventory.Instance.AddResources(burntIngredient, 1);
    else
      Ingredient_Inventory.Instance.AddResources(cookedIngredientData, 1); // Fallback to cooked ingredient (for now)

    // Reset pan for next use
    Invoke(nameof(ResetAll), 1f); // Delay reset to allow player to see result
  }

  /// <summary>
  /// Starts the cooking slider minigame.
  /// </summary>
  public void StartSlider()
  {
    slider.SetActive(true);
    sliderActive = true;
    sliderComponent.value = 0f;
  }

  /// <summary>
  /// Starts the second cooking slider minigame after the ingredient fall section.
  /// </summary>
  public void StartSecondSlider()
  {
    regularPan.SetActive(true);
    draggablePan.SetActive(false);
    ingrInPanImage.gameObject.SetActive(true);
    sliderComponent.value = 0f;
    if (firstCookedState == CookedState.Cooked || firstCookedState == CookedState.Overcooked || firstCookedState == CookedState.Almost)
    { // For now, treat Almost and Overcooked as Cooked
      ingrInPanImage.sprite = cookedIngredientData.Image;
      // ingrInPanImage.preserveAspect = true;
    }
    else if (firstCookedState == CookedState.Burnt)
    {
      ingrInPanImage.color = Color.black; // Burnt ingredient shown as blacked out image
      // ingrInPanImage.preserveAspect = false;
    }
    else // It should still be raw, so I don't think this is necessary?
    {
      ingrInPanImage.sprite = ingredientInPan.Image;
      // ingrInPanImage.preserveAspect = true;
    }
    SetImageAlpha(ingrInPanImage, 1f);
    Invoke(nameof(StartSlider), 0.5f); // Small delay before starting second slider
  }

  /// <summary>
  /// Sets image alpha. Used whenever an image's color is changed because it seems to make it
  /// completely transparent if alpha is not set.
  /// </summary>
  private void SetImageAlpha(Image img, float alpha)
  {
    Color c = img.color;
    c.a = alpha;
    img.color = c;
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
