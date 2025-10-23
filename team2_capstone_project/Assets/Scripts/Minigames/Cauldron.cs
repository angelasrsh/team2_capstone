using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using Grimoire;
using UnityEngine;
using UnityEngine.UI;

public class Cauldron : MonoBehaviour
{
    private Dictionary<Ingredient_Data, int> ingredientInPot = new Dictionary<Ingredient_Data, int>();
    private List<Dish_Data> possibleDishes; // not a deep copy, but clearing this won't affect original list in Ingredient_Data
    private List<Ingredient_Requirement> possibleIngredients;
    [SerializeField] private GameObject errorText;

    [Header("Water UI Particles")]
    public CanvasGroup waterCanvasGroup;
    public UIParticle waterParticle1;
    public UIParticle waterParticle2;

    private Dish_Data dishMade;
    private Ingredient_Data ingredientMade;
    private bool stirring = false;

    // [Header("Stirring Progress UI")]
    // private bool isPlayerMoving = false;
    // private float currentElapsedTime = 0f;
    // private float currentStirDuration = 0f;
    // private bool hasFinished = false;

    /// <summary>
    /// Check the current ingredients in the pot against possible recipes and create the appropriate dish.
    /// Adds the created dish to the Dish_Tool_Inventory and clears the pot.
    /// This is called in FinishedStir after the stirring time is completed.
    /// </summary>
    private void CheckRecipeAndCreateDish()
    {
        Dish_Data matchedDish = null;
        List<Dish_Data> dishesToCheck = possibleDishes;
        if (dishesToCheck != null)
        {
            if (possibleDishes == null || possibleDishes.Count == 0)
                dishMade = Game_Manager.Instance.dishDatabase.GetBadDish();
            else
            {
                foreach (var dish in dishesToCheck)
                {
                    bool allReqsSatisfied = true;

                    foreach (var req in dish.ingredientQuantities)
                    {
                        // Require that pot contains the ingredient with at least the required amount.
                        if (!ingredientInPot.TryGetValue(req.ingredient, out int haveAmount) || haveAmount != req.amountRequired)
                        {
                            allReqsSatisfied = false;
                            break;
                        }
                    }

                    if (allReqsSatisfied)
                    {
                        if (dish.recipe == Recipe.Cauldron)
                        {
                            matchedDish = dish;
                            break;
                        }
                        allReqsSatisfied = false; // dish is not made using cauldron; continue
                    }
                }   
            }
        }

        Ingredient_Data matchedIngredient = null;
        if (matchedDish == null && possibleIngredients != null && possibleIngredients.Count > 0)
        {
            List<Ingredient_Requirement> ingredientsToCheck = possibleIngredients;

            if (ingredientsToCheck != null)
            {
                foreach (var ingrReq in ingredientsToCheck)
                {
                    var ingredientCandidate = ingrReq.ingredient;
                    bool allReqsSatisfied = true;

                    foreach (var req in ingredientCandidate.ingredientsNeeded)
                    {
                        if (!ingredientInPot.TryGetValue(req.ingredient, out int haveAmount) || haveAmount < req.amountRequired)
                        {
                            allReqsSatisfied = false;
                            break;
                        }
                    }

                    if (allReqsSatisfied)
                    {
                        if (ingrReq.method == Recipe.Cauldron)
                        {
                            matchedIngredient = ingredientCandidate;
                            break;
                        }
                        allReqsSatisfied = false; // ingredientCandidate isn't made in cauldron; continue
                    }
                }
            }
        }

        // Add to inventory immediately (synchronously)
        if (matchedIngredient != null)
        {
            ingredientMade = matchedIngredient;
            Debug.Log("[Cauldron] Ingredient made: " + ingredientMade.Name);
            Ingredient_Inventory.Instance.AddResources(Ingredient_Inventory.Instance.IngrDataToEnum(ingredientMade), 1);
            Completed_Dish_UI_Popup_Manager.instance?.ShowPopup($"{ingredientMade.Name} Created!", Color.white);
        }
        else if (matchedDish != null)
        {
            dishMade = matchedDish;
            Debug.Log("[Cauldron] Dish made: " + dishMade.Name);
            Dish_Tool_Inventory.Instance.AddResources(dishMade, 1);
            Completed_Dish_UI_Popup_Manager.instance?.ShowPopup($"{dishMade.Name} Created!", Color.red);
        }
        else
        {
            Debug.Log("[Cauldron] No recipe found with ingredients provided. Making bad dish.");
            dishMade = Game_Manager.Instance.dishDatabase.GetBadDish();
            Dish_Tool_Inventory.Instance.AddResources(dishMade, 1);
            Completed_Dish_UI_Popup_Manager.instance?.ShowPopup("Failed dish...", Color.black);
        }
        ResetAll();
    }

    /// <summary>
    /// Resets all lists and variables to start new recipe detection.
    /// </summary>
    public void ResetAll()
    {
        ingredientInPot.Clear();
        possibleDishes?.Clear();
        possibleIngredients?.Clear();
        dishMade = null;
        ingredientMade = null;
    }

    #region Stirring Control
    public void StartStirring()
    {
        stirring = true;
    }

    /// <summary>
    /// Called once when progress reaches 100%.
    /// </summary>
    public void FinishedStir()
    {
        stirring = false;
        if (dishMade != null)
        {
            Debug.Log("Bad dish made due first ingredient leading to no known recipes.");
            dishMade = Game_Manager.Instance.dishDatabase.GetBadDish();
            Dish_Tool_Inventory.Instance.AddResources(dishMade, 1);
            ResetAll();
            return;
        }

        CheckRecipeAndCreateDish();
    }
    #endregion


    public bool IsStirring() => stirring;
    public void DisableErrorTextAfterDelay() => Invoke("HideErrorText", 3f);
    public void HideErrorText() => errorText?.SetActive(false);


    #region Add/Remove Ingredients
    public void AddToPot(Ingredient_Data ingredient)
    {
        if (ingredientInPot.ContainsKey(ingredient))
            ingredientInPot[ingredient]++;
        else
            ingredientInPot[ingredient] = 1;

        if (ingredient.Name == "Water")
        {
            Audio_Manager.instance.AddWater();
        }
        else
        {
            Audio_Manager.instance.AddOneIngredient();
            if (waterParticle1 != null && waterParticle2 != null)
            {
                StartCoroutine(ShowWaterParticlesCoroutine());
            }
        }

        if (ingredientInPot.Count == 1)
        {
            possibleDishes = new List<Dish_Data>(ingredient.usedInDishes);
            possibleIngredients = new List<Ingredient_Requirement>(ingredient.makesIngredient);

            if (possibleDishes.Count == 0 && possibleIngredients.Count == 0)
            {
                Debug.Log("No dishes or ingredients are made with this ingredient, making bad dish");
                dishMade = Game_Manager.Instance.dishDatabase.GetBadDish();
            }
        }

        Debug.Log("Different ingredients in pot: " + ingredientInPot.Count);
    }

    public void RemoveFromPot(Ingredient_Data ingredient)
    {
        if (ingredientInPot.ContainsKey(ingredient))
        {
            ingredientInPot[ingredient]--;
            if (ingredientInPot[ingredient] <= 0)
                ingredientInPot.Remove(ingredient);
        }

        Debug.Log("Ingredients in pot: " + ingredientInPot.Count);
    }

    public bool IsEmpty() => ingredientInPot.Count == 0;

    public bool AddWater(Ingredient_Data water)
    {
        if (water == null)
        {
            Debug.LogWarning("[Cauldron] AddWater called with null water.");
            return false;
        }

        if (Drag_All.IsWaterAdded())
        {
            Debug.Log("[Cauldron] Water already added.");
            return false;
        }

        AddToPot(water);
        Audio_Manager.instance?.PlayBubblingOnLoop();
        return true;
    }

    private IEnumerator ShowWaterParticlesCoroutine()
    {
        waterCanvasGroup.alpha = 1f;
        waterParticle1.Play();
        waterParticle2.Play();

        yield return new WaitForSeconds(1f);
        
        waterCanvasGroup.alpha = 0f;
        waterParticle1.Stop();
        waterParticle2.Stop();
    }
    #endregion
}
