using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory_Overlap : MonoBehaviour, ICustomDrag
{
    private RectTransform rectTransform;
    [SerializeField] RectTransform redZone;

    private static List<Inventory_Overlap> ingredientOnPot = new List<Inventory_Overlap>();
    [SerializeField] GameObject dishPrefab;
    private bool isOnPot = false;

    public void OnCurrentDrag()
    {
        rectTransform.position = Input.mousePosition;
        if (DragAll.IsOverlapping(rectTransform, redZone))
        {
            Debug.Log("In RED");
            if (!isOnPot)
            {
                AddToPot();
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void AddToPot()
    {
        isOnPot = true;
        ingredientOnPot.Add(this);
        Debug.Log("Eggs on pot: " + ingredientOnPot.Count);

        if (ingredientOnPot.Count >= 2)
        {
            CreateDish(); //only gets the position if the second egg is placed
        }
    }

    private void CreateDish()
    {
        Debug.Log("Creating Dish");

        Vector3 dishPosition = this.rectTransform.position;
        Transform parentTransform = this.rectTransform.parent;

        List<Inventory_Overlap> ingredientsToDestroy = new List<Inventory_Overlap>(ingredientOnPot);
        ingredientOnPot.Clear();


        foreach (var ingredient in ingredientsToDestroy)
        {
            if (ingredient != null)
            {
                Destroy(ingredient.gameObject);
            }
        }
        // Create the new dish at the second egg's position
        if (dishPrefab != null)
        {
            GameObject newDish = Instantiate(dishPrefab, dishPosition, Quaternion.identity);
            // Make sure it's a child of the same parent as the egg
            newDish.transform.SetParent(parentTransform, false);

            // Since we're using world position, we need to set it after parenting
            newDish.transform.position = dishPosition;
            newDish.transform.SetAsLastSibling(); // This will put it on top

        }
    }

    // Clean up when object is destroyed
    void OnDestroy()
    {
        if (isOnPot)
        {
            ingredientOnPot.Remove(this);
        }
    }
    
    private void RemoveFromPot()
    {
        isOnPot = false;
        ingredientOnPot.Remove(this);
        Debug.Log("Ingredients on pot: " + ingredientOnPot.Count);
    }
}
