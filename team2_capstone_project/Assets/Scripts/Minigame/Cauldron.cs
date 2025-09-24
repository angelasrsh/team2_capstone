using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
  private Dictionary<Ingredient_Data, int> ingredientInPot = new Dictionary<Ingredient_Data, int>();
  private List<Dish_Data> possibleDishes; // not a deep copy, but clearing this won't affect original list in Ingredient_Data
  private List<Ingredient_Requirement> possibleIngredients;
  // private bool badDishMade = false;
  private Dish_Data dishMade;
  private Ingredient_Data ingredientMade;
  private bool stirring = false;

  /// <summary>
  /// Call this when the stirring time is finished (e.g., dish is ready to be made).
  /// This should be called in Drag_All when the stirring time is finished.
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

    // Shouldn't get here if already making bad dish
    Debug.Log("Finished Stir. Checking Recipe.");
    CheckRecipeAndCreateDish();
  }

    /// <summary>
    /// Check the current ingredients in the pot against possible recipes and create the appropriate dish.
    /// Adds the created dish to the Dish_Tool_Inventory and clears the pot.
    /// This is called in FinishedStir after the stirring time is completed.
    /// </summary>
    private void CheckRecipeAndCreateDish()
    {
        // Loop through possible dishes and see if any can be made with current ingredients
        Dish_Data potentialDish = null;
        foreach (var dish in possibleDishes)
        {
            if (ingredientInPot.Count != dish.ingredientQuantities.Count)
                continue;

            potentialDish = dish;
            foreach (var req in dish.ingredientQuantities)
            {
                if (!ingredientInPot.ContainsKey(req.ingredient) || ingredientInPot[req.ingredient] < req.amountRequired)
                {
                    potentialDish = null;
                    break;
                }
            }

            if (potentialDish != null) // Found a matching dish recipe
            {
                dishMade = potentialDish;
                break;
            }
        }

        Ingredient_Data potentialIngredient = null;
        if (dishMade == null)
        {
            // Loop through possible ingredients and see if any can be made with current ingredients
            foreach (var ingredient in possibleIngredients)
            {
                if (ingredientInPot.Count != ingredient.ingredient.ingredientsNeeded.Count)
                    continue;

                potentialIngredient = ingredient.ingredient;
                foreach (var req in potentialIngredient.ingredientsNeeded)
                {
                    if (!ingredientInPot.ContainsKey(req.ingredient) || ingredientInPot[req.ingredient] < req.amountRequired)
                    {
                        potentialIngredient = null;
                        break;
                    }
                }

                if (potentialIngredient != null) // Found a matching ingredient recipe
                {
                    ingredientMade = potentialIngredient;
                    break;
                }
            }
        }

        if (ingredientMade != null)
        {
            Debug.Log("Ingredient made: " + ingredientMade.Name);
            Ingredient_Inventory.Instance.AddResources(Ingredient_Inventory.Instance.IngrDataToEnum(ingredientMade), 1);
        }
        else if (dishMade != null)
        {
            Debug.Log("Dish made: " + dishMade.Name);
            Dish_Tool_Inventory.Instance.AddResources(dishMade, 1);
        }
        else if (dishMade == null)
        {
            Debug.Log("No recipe found with ingredients provided. Made bad dish.");
            dishMade = Game_Manager.Instance.dishDatabase.GetBadDish();
            Dish_Tool_Inventory.Instance.AddResources(dishMade, 1);
        }

        ResetAll();

        // Broadcast that a dish has been made (regardless of success)
        Game_Events_Manager.Instance.MakeCauldronDish();
  }

  /// <summary>
  /// Resets all lists and variables to start new recipe detection.
  /// </summary>
  private void ResetAll()
  {
    ingredientInPot.Clear();
    possibleDishes.Clear();
    possibleIngredients.Clear();
    dishMade = null;
    ingredientMade = null;
  }

  /// <summary>
  /// Call this when the stirring action starts (e.g., player starts stirring the cauldron).
  /// Use stirring boolean to prevent new ingredients from being added from this point on until a dish is made.
  /// Essentially, the player can only add ingredients before they start stirring.
  /// This should be called in Drag_All where the stirring drag action is detected.
  /// </summary>
  public void StartStirring()
  {
    stirring = true;
  }

  public bool IsStirring()
  {
    return stirring;
  }

  /// <summary>
  /// Call this to add an ingredient to the pot (e.g., when an ingredient is dragged into the pot area).
  /// This should be called in Drag_All when an ingredient is dragged into the pot area.
  /// If this is the first ingredient, it initializes the possibleDishes list based on the ingredient's usedInDishes.
  /// If the ingredient has no usedInDishes, it sets badDishMade to true.
  /// </summary>
  public void AddToPot(Ingredient_Data ingredient)
  {
    if (ingredientInPot.ContainsKey(ingredient))
      ingredientInPot[ingredient]++;
    else
      ingredientInPot[ingredient] = 1;

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

  /// <summary>
  /// Call this to remove an ingredient from the pot (e.g., when an ingredient is dragged out of the pot area).
  /// This should be called in Drag_All when an ingredient is dragged out of the pot area
  /// </summary>
  /// <param name="ingredient"></param>
  public void RemoveFromPot(Ingredient_Data ingredient)
  {
    if (ingredientInPot.ContainsKey(ingredient))
    {
      ingredientInPot[ingredient]--;
      if (ingredientInPot[ingredient] <= 0)
        ingredientInPot.Remove(ingredient);
    }
    Debug.Log("Ingredients on pot: " + ingredientInPot.Count);
  }

  public bool IsEmpty()
  {
    return ingredientInPot.Count == 0;
  }
}