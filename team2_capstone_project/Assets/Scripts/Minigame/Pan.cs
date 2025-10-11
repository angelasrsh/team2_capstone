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
  [SerializeField] private GameObject inventoryCanvas; // reference to the inventory canvas to re-enable after minigame
  [SerializeField] private GameObject dishInventoryCanvas; // reference to the dish inventory canvas to re-enable after minigame
  public enum CookedState
  {
    Raw,
    Cooked,
    Burnt
  }

  public void Start()
  {
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
    return true;
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
