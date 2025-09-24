using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Code for one inventory slot in an inventory grid.
/// 
/// To use this class, add an Inventory_Grid prefab (which contains inventory_slots with this code).
///
/// Required to be on an Inventory Slot with children gameObjects for Name, Amount, and Image
/// Would like to improve this so that it's not an issue, but I don't know how yet.
/// </summary>
public class Inventory_Slot : MonoBehaviour
{
    public Item_Stack stk;

    /// <summary>
    /// Fill the item slot with the given number of items. Called by Inventory_Grid.
    /// </summary>
    /// <param name="stack"> An Item_Stack given by inventory with which to fill this slot </param>
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
            stk.resource = stack.resource;
            stk.amount = stack.amount;

        }
        else
        {
            itemImage.sprite = null;
            name.text = null;
            amount.text = null;
            itemImage.enabled = false;
            stk.resource = null;
            stk.amount = 0;
        }


        // If it has an Drag_All script and the resource is not null, assign the type
        Drag_All script = image_slot.GetComponent<Drag_All>();
        if (script != null)
        {
            if (stack != null && stack.resource != null && stack.resource is Ingredient_Data)
                script.SetIngredientType((Ingredient_Data)stack.resource);
            else
                script.SetIngredientType(null);
        }

    }
}
