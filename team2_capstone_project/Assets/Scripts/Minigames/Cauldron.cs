using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.UI;

public class Cauldron : MonoBehaviour
{
    private Dictionary<Ingredient_Data, int> ingredientInPot = new Dictionary<Ingredient_Data, int>();
    private List<Dish_Data> possibleDishes; // not a deep copy, but clearing this won't affect original list in Ingredient_Data
    private List<Ingredient_Requirement> possibleIngredients;
    [SerializeField] private GameObject errorText;

    private Dish_Data dishMade;
    private Ingredient_Data ingredientMade;
    private bool stirring = false;

    [Header("Stirring Progress UI")]
    [SerializeField] private Slider stirProgressBar;
    private Coroutine stirProgressRoutine;

    private bool isPlayerMoving = false;
    private float currentElapsedTime = 0f;
    private float currentStirDuration = 0f;
    private bool hasFinished = false;

    /// <summary>
    /// Check the current ingredients in the pot against possible recipes and create the appropriate dish.
    /// Adds the created dish to the Dish_Tool_Inventory and clears the pot.
    /// This is called in FinishedStir after the stirring time is completed.
    /// </summary>
    private void CheckRecipeAndCreateDish()
    {
        // Debug the current pot contents
        Debug.Log("[Cauldron] Checking recipes. Pot contents:");
        foreach (var kv in ingredientInPot)
        {
            Debug.Log($" - {kv.Key.Name}: {kv.Value}");
        }

        Dish_Data matchedDish = null;

        // If possibleDishes is null or empty, fallback to scanning full recipe DB if available.
        IEnumerable<Dish_Data> dishesToCheck = possibleDishes != null && possibleDishes.Count > 0
            ? possibleDishes
            : Game_Manager.Instance != null ? Game_Manager.Instance.dishDatabase.GetAllDishes() : null;

        if (dishesToCheck != null)
        {
            foreach (var dish in dishesToCheck)
            {
                bool allReqsSatisfied = true;

                foreach (var req in dish.ingredientQuantities)
                {
                    // Require that pot contains the ingredient with at least the required amount.
                    if (!ingredientInPot.TryGetValue(req.ingredient, out int haveAmount) || haveAmount < req.amountRequired)
                    {
                        allReqsSatisfied = false;
                        break;
                    }
                }

                if (allReqsSatisfied)
                {
                    matchedDish = dish;
                    Debug.Log($"[Cauldron] Matched dish: {dish.Name}");
                    break;
                }
                else
                {
                    Debug.Log($"[Cauldron] Dish '{dish.Name}' did not match (requirements not satisfied).");
                }
            }
        }
        else
        {
            Debug.LogWarning("[Cauldron] possibleDishes is null/empty and no dish DB found to fallback to.");
        }

        Ingredient_Data matchedIngredient = null;

        if (matchedDish == null)
        {
            IEnumerable<Ingredient_Requirement> ingredientsToCheck = possibleIngredients != null && possibleIngredients.Count > 0
                ? possibleIngredients
                : null;

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
                        matchedIngredient = ingredientCandidate;
                        Debug.Log($"[Cauldron] Matched ingredient: {matchedIngredient.Name}");
                        break;
                    }
                    else
                    {
                        Debug.Log($"[Cauldron] Ingredient '{ingredientCandidate.Name}' did not match.");
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

    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ResetAll();
        Debug.Log("[Cauldron] ResetAll after dish creation.");
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

        // Reset progress bar UI
        if (stirProgressBar != null)
        {
            stirProgressBar.value = 0f;
            stirProgressBar.gameObject.SetActive(false);
        }

        // important to clear timing vars so next stir behaves normally
        currentElapsedTime = 0f;
        currentStirDuration = 0f;

        Debug.Log("[Cauldron] ResetAll: pot cleared.");
    }

    #region Stirring Control
    public void StartStirring(float totalDuration)
    {
        currentStirDuration = totalDuration;

        // If completed previously, reset finished flag for a new stir
        hasFinished = false;

        if (stirProgressRoutine == null)
            stirProgressRoutine = StartCoroutine(UpdateStirProgress());

        stirring = true;
        isPlayerMoving = true; // assume movement when entering red zone

        if (stirProgressBar != null)
        {
            stirProgressBar.gameObject.SetActive(true);
            stirProgressBar.value = Mathf.Clamp01(currentElapsedTime / Mathf.Max(0.0001f, currentStirDuration));
        }

        Debug.Log("[Cauldron] StartStirring: duration=" + currentStirDuration + " elapsed=" + currentElapsedTime);
    }

    public void PauseStirring()
    {
        if (hasFinished) return;
        stirring = false;
        isPlayerMoving = false;
        Debug.Log("[Cauldron] PauseStirring: elapsed=" + currentElapsedTime);
    }

    public void ResumeStirring()
    {
        if (hasFinished) return;

        if (stirProgressRoutine == null)
            stirProgressRoutine = StartCoroutine(UpdateStirProgress());

        stirring = true;
        isPlayerMoving = true;
        Debug.Log("[Cauldron] ResumeStirring: elapsed=" + currentElapsedTime);
    }

    public void StopStirringCompletely()
    {
        // Stop coroutine entirely and leave UI in the paused state,
        // but do not re-enable UI if already finished 
        if (stirProgressRoutine != null)
        {
            StopCoroutine(stirProgressRoutine);
            stirProgressRoutine = null;
        }

        stirring = false;
        isPlayerMoving = false;

        if (stirProgressBar != null && !hasFinished)
        {
            // Keep whatever progress exists and show the bar as a paused state
            stirProgressBar.value = Mathf.Clamp01(currentElapsedTime / Mathf.Max(0.0001f, currentStirDuration));
            stirProgressBar.gameObject.SetActive(true);
        }

        Debug.Log("[Cauldron] StopStirringCompletely: coroutine stopped, elapsed=" + currentElapsedTime + " hasFinished=" + hasFinished);
    }

    private IEnumerator UpdateStirProgress()
    {
        while (true)
        {
            if (stirring && isPlayerMoving && !hasFinished)
            {
                currentElapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(currentElapsedTime / Mathf.Max(0.0001f, currentStirDuration));

                if (stirProgressBar != null)
                    stirProgressBar.value = progress;

                if (progress >= 1f)
                {
                    FinishedStir();
                    yield break;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Called once when progress reaches 100%.
    /// </summary>
    public void FinishedStir()
    {
        if (hasFinished)
        {
            Debug.Log("[Cauldron] FinishedStir called but already finished — ignoring duplicate call.");
            return;
        }

        hasFinished = true;

        // stop any running progress coroutine
        if (stirProgressRoutine != null)
        {
            StopCoroutine(stirProgressRoutine);
            stirProgressRoutine = null;
        }

        // ensure UI shows full then hide
        if (stirProgressBar != null)
        {
            stirProgressBar.value = 1f;
            stirProgressBar.gameObject.SetActive(false); // hide after completion
        }

        Debug.Log("[Cauldron] FinishedStir: recipe complete, invoking CheckRecipeAndCreateDish()");

        CheckRecipeAndCreateDish();
        StartCoroutine(ResetAfterDelay(0.05f));
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

    public void SetPlayerStirring(bool isMoving) => isPlayerMoving = isMoving;

    #endregion
}
