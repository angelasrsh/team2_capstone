using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum IngredientType // should delete after putting in maddie's code
{
    Egg,
    Melon
}
public class Inventory_Overlap : MonoBehaviour, ICustomDrag
{
    private RectTransform rectTransform;
    [SerializeField] RectTransform redZone;
    [SerializeField] IngredientType ingredientType; // Set this in Inspector for each ingredient
    [SerializeField] GameObject goodDishPrefab; // For egg + egg
    [SerializeField] GameObject badDishPrefab;  // For egg + melon

    public AudioSource goodDishMade;
    private static List<Inventory_Overlap> ingredientOnPot = new List<Inventory_Overlap>();
    private bool isOnPot = false;
    private Inventory playerInventory;
    public Dish_Data DishData;

    public void OnCurrentDrag()
    {
        rectTransform.position = Input.mousePosition;
        if (Drag_All.IsOverlapping(rectTransform, redZone))
        {
          Debug.Log("In RED");
          if (!isOnPot)
            AddToPot();
        }
        else
        {
          
          if (isOnPot)
            RemoveFromPot();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
      rectTransform = GetComponent<RectTransform>();
      playerInventory = FindObjectOfType<Inventory>()?.GetComponent<Inventory>();
    }

    private void AddToPot()
    {
      isOnPot = true;
      ingredientOnPot.Add(this);
      Debug.Log("Ingredients on pot: " + ingredientOnPot.Count);

      if (ingredientOnPot.Count >= 2)
        CheckRecipeAndCreateDish(); //only gets the position if the second egg is placed
    }

    private void CheckRecipeAndCreateDish()
    {
      if (ingredientOnPot.Count < 2) return;
      IngredientType firstType = ingredientOnPot[0].ingredientType;
      IngredientType secondType = ingredientOnPot[1].ingredientType;
      Debug.Log("Creating Dish");

      Vector3 dishPosition = redZone.transform.position;
      Transform parentTransform = this.rectTransform.parent;
      GameObject dishToCreate = null;


      // Recipe logic
      if (firstType == IngredientType.Egg && secondType == IngredientType.Egg)
      {
        dishToCreate = goodDishPrefab;
        Debug.Log("Creating good dish: Egg + Egg!");


        goodDishMade.PlayOneShot(goodDishMade.clip);
      }
      else if ((firstType == IngredientType.Egg && secondType == IngredientType.Melon) ||
               (firstType == IngredientType.Melon && secondType == IngredientType.Egg))
      {
        dishToCreate = badDishPrefab;
        Debug.Log("Creating bad dish: Egg + Melon!");
      }
      else
      {
        Debug.Log("Unknown recipe combination!");
        dishToCreate = badDishPrefab; // Default to bad dish
      }

      // Destroy both ingredient objects
      List<Inventory_Overlap> ingredientToDestroy = new List<Inventory_Overlap>(ingredientOnPot);
      ingredientOnPot.Clear();

      foreach (var ingredient in ingredientToDestroy)
      {
        if (ingredient != null)
          Destroy(ingredient.gameObject);
      }

      // Create the new dish at the second egg's position
      if (dishToCreate != null)
      {
        GameObject newDish = Instantiate(dishToCreate, dishPosition, Quaternion.identity);
        // Make sure it's a child of the same parent as the egg
        newDish.transform.SetParent(parentTransform, false);

        // Since we're using world position, we need to set it after parenting
        newDish.transform.position = dishPosition;
        newDish.transform.SetAsLastSibling(); // This will put it on top
      }

      if (playerInventory != null && DishData != null)
      {
        playerInventory.AddDish(DishData);
        Debug.Log($"[Overlap] Added {DishData.Name} to inventory");
      }
      else
        Debug.Log("[Overlap] Missing Inventory or Dish_Data reference!");
    }

    // Clean up when object is destroyed
    void OnDestroy()
    {
      if (isOnPot)
        ingredientOnPot.Remove(this);
    }

    private void RemoveFromPot()
    {
      isOnPot = false;
      ingredientOnPot.Remove(this);
      Debug.Log("Ingredients on pot: " + ingredientOnPot.Count);
    }
}
