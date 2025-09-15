using System.Collections;
using System.Collections.Generic;
// using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
// using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Required to be on an Inventory Slot with children gameObjects for Name, Amount, and Image
/// Would like to improve this so that it's not an issue, but I don't know how yet
/// </summary>
public class Inventory_Slot : MonoBehaviour
{
    public Item_Stack stk;

    /// <summary>
    /// Fill the item slot with the given number of items
    /// </summary>
    /// <param name="stack"></param>
    public void PopulateSlot(Item_Stack stack)
    {

        GameObject image_slot = transform.Find("Image_Slot").gameObject;
        UnityEngine.UI.Image itemImage = image_slot.GetComponent<UnityEngine.UI.Image>();

        TextMeshProUGUI name = transform.Find("Name").gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI amount = transform.Find("Amount").gameObject.GetComponent<TextMeshProUGUI>();

        if (itemImage == null || name == null || amount == null)
            Debug.LogError($"[Invtry_Slts] Error: Slot missing a required component");

        // Populate values or fill with nothing if null
        if (stack != null && stack.resource != null)
        {
            itemImage.enabled = true;
            itemImage.sprite = stack.resource.Image;
            name.text = stack.resource.Name;
            amount.text = stack.amount.ToString();
            
        }
        else
        {
            itemImage.sprite = null;
            name.text = null;
            amount.text = null;
            itemImage.enabled = false;
        }

 
        // If it has an Inventory_Overlap script and the resource is not null, assign the type
        Inventory_Overlap script = image_slot.GetComponent<Inventory_Overlap>();
        if (script != null)
        {
            if (stack != null && stack.resource != null)
                script.SetIngredientType((Ingredient_Data)stack.resource);
            else
                script.SetIngredientType(null);
        }
               




    }
}
