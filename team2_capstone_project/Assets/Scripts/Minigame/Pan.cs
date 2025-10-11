using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
  private Ingredient_Data ingredientInPan;
  private Ingredient_Data cookedIngredientData;
  private CookedState firstCookedState = CookedState.Raw;
  private CookedState secondCookedState = CookedState.Raw;
  [SerializeField] private GameObject errorText;
  public enum CookedState
  {
    Raw,
    Cooked,
    Burnt
  }

  /// <summary>
  /// Resets frying pan to initial state.
  /// </summary>
  public void ResetAll()
  {
    ingredientInPan = null;
    cookedIngredientData = null;
    firstCookedState = CookedState.Raw;
    secondCookedState = CookedState.Raw;
  }

  /// <summary>
  /// Call this to add the ingredient to the pan. This should be called in Drag_All when 
  /// an ingredient is dropped into the pan's red zone.
  /// </summary>
  public void AddToPan(Ingredient_Data ingredient)
  {
    if (ingredientInPan != null)
    {
      Debug.Log("[Pan]: Pan already has an ingredient. Cannot add another.");
      errorText.GetComponent<TMPro.TMP_Text>().text = "Pan already has an ingredient. Cannot add another.";
      errorText.SetActive(true);
      Invoke(nameof(HideErrorText), 3);
      return;
    }

    // Debug.Log("[Pan]: Added " + ingredient.Name + " to pan.");
    ingredientInPan = ingredient;
    cookedIngredientData = ingredientInPan;

    // SFX
    // if (ingredient.Name == "Water")
    // {
    //     Audio_Manager.instance.AddWater();
    // }
    // else
    //   Audio_Manager.instance.AddOneIngredient();
  }

  // public void DisableErrorTextAfterDelay()
  // {
  //   Invoke("HideErrorText", 3f);
  // }

  public void HideErrorText()
  {
    errorText?.SetActive(false);
  }
}
