using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory_Overlap : MonoBehaviour, ICustomDrag
{
    private RectTransform rectTransform;
    [SerializeField] RectTransform redZone;
    [SerializeField] IngredientType ingredientType; // Set in code by parent Inventory_Slot
    [SerializeField] GameObject goodDishPrefab; // For egg + egg
    [SerializeField] GameObject badDishPrefab;  // For egg + melon

    public AudioSource goodDishMade;
    private static List<Inventory_Overlap> ingredientOnPot = new List<Inventory_Overlap>();
    private bool isOnPot = false;
    //private Inventory playerInventory;
    public Dish_Data DishData;
    private Vector3 originalPosition;

    public Inventory_Slot ParentSlot; // Since the parent is the UI Canvas otherwise
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // playerInventory = FindObjectOfType<Inventory>()?.GetComponent<Inventory>();

        // Set red zone if in cauldron scene
        GameObject zonedefine = GameObject.Find("Image-Pot");
        if (zonedefine != null)
            redZone = zonedefine.GetComponent<RectTransform>();
        else
            Debug.Log("[Invty_Ovlrp] Could not find Image-Pot for redZone!");

        GameObject goodDishGameObject = GameObject.Find("MadeGoodDish");
        if (goodDishGameObject == null)
            Debug.Log("[Intry_Ovlrp] Could not find MadeGoodDish!");
        else
            goodDishMade = goodDishGameObject.GetComponent<AudioSource>();
    }

    public void startDrag()
    {
        // save starting position of rectTransform
        originalPosition = rectTransform.position;
    }

    public void OnCurrentDrag()
    {
        rectTransform.position = Input.mousePosition;
    }

    public void EndDrag()
    {
        if (Drag_All.IsOverlapping(rectTransform, redZone))
        {
            Debug.Log("In RED");
            if (!isOnPot)
            {
                AddToPot();
                // The Inventory UI requires an image slot, so duplicate and replace self
                GameObject newImageSlot = Instantiate(this.gameObject, ParentSlot.transform);
                this.name = "Image_Slot_Old";
                newImageSlot.name = "Image_Slot"; // Must rename so Inventory_Slot can find the new image_slot
                newImageSlot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

                Ingredient_Inventory.Instance.RemoveResources(ingredientType, 1);
            }

        }
        else
        {
            if (isOnPot)
                RemoveFromPot();
            else
            {
                Debug.Log("Not in RED, snapping back");
                rectTransform.position = originalPosition;
            }
        }
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

        if (DishData != null)
        {
            //playerInventory.AddDish(DishData);
            if (Dish_Inventory.Instance.AddResources(DishData, 1) < 1)
            {
                Debug.Log($"[Intry_Ovrlp] Error: no {DishData} added!");
                return;
            }

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

    /// <summary>
    /// For another object to set this image_slot's ingredient type (used in inventory UI)
    /// </summary>
    /// <param name="iData"></param>
    public void SetIngredientType(Ingredient_Data iData)
    {
        ingredientType = Ingredient_Inventory.Instance.IngrDataToEnum(iData);
    }
}
