using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
    private static List<Ingredient_Data> ingredientOnPot = new List<Ingredient_Data>();
    public void AddToPot(Ingredient_Data ingredient)
    {
        ingredientOnPot.Add(ingredient);
        Debug.Log("Ingredients on pot: " + ingredientOnPot.Count);
    }
    
    public void stir()
    {
        // if (ingredientOnPot.Count >= 2)
        //     CheckRecipeAndCreateDish(); //only gets the position if the second egg is placed
    }
    
    private void CheckRecipeAndCreateDish()
    {
        // if (ingredientOnPot.Count < 2) return;
        // IngredientType firstType = ingredientOnPot[0].ingredientType;
        // IngredientType secondType = ingredientOnPot[1].ingredientType;
        // Debug.Log("Creating Dish");

        // Vector3 dishPosition = redZone.transform.position;
        // Transform parentTransform = this.rectTransform.parent;
        // GameObject dishToCreate = null;


        // // Recipe logic
        // if (firstType == IngredientType.Egg && secondType == IngredientType.Egg)
        // {
        //     dishToCreate = goodDishPrefab;
        //     Debug.Log("Creating good dish: Egg + Egg!");


        //     goodDishMade.PlayOneShot(goodDishMade.clip);
        // }
        // else if ((firstType == IngredientType.Egg && secondType == IngredientType.Melon) ||
        //          (firstType == IngredientType.Melon && secondType == IngredientType.Egg))
        // {
        //     dishToCreate = badDishPrefab;
        //     Debug.Log("Creating bad dish: Egg + Melon!");
        // }
        // else
        // {
        //     Debug.Log("Unknown recipe combination!");
        //     dishToCreate = badDishPrefab; // Default to bad dish
        // }

        // // Destroy both ingredient objects
        // List<Ingredient_Data> ingredientToDestroy = new List<Ingredient_Data>(ingredientOnPot);
        // ingredientOnPot.Clear();

        // foreach (var ingredient in ingredientToDestroy)
        // {
        //     if (ingredient != null)
        //         Destroy(ingredient.gameObject);
        // }
        // ingredientOnPot.Clear();

        // // Create the new dish at the second egg's position
        // if (dishToCreate != null)
        // {
        //     GameObject newDish = Instantiate(dishToCreate, dishPosition, Quaternion.identity);
        //     // Make sure it's a child of the same parent as the egg
        //     newDish.transform.SetParent(parentTransform, false);

        //     // Since we're using world position, we need to set it after parenting
        //     newDish.transform.position = dishPosition;
        //     newDish.transform.SetAsLastSibling(); // This will put it on top
        // }

        // if (DishData != null)
        // {
        //     //playerInventory.AddDish(DishData);
        //     if (Dish_Inventory.Instance.AddResources(DishData, 1) < 1)
        //     {
        //         Debug.Log($"[Intry_Ovrlp] Error: no {DishData} added!");
        //         return;
        //     }

        //     Debug.Log($"[Overlap] Added {DishData.Name} to inventory");
        // }
        // else
        //     Debug.Log("[Overlap] Missing Inventory or Dish_Data reference!");
    }

    public void RemoveFromPot(Ingredient_Data ingredient)
    {
        // isOnPot = false;
        ingredientOnPot.Remove(ingredient);
        Debug.Log("Ingredients on pot: " + ingredientOnPot.Count);
    }
}
