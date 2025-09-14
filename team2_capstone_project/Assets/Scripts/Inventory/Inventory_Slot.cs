using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Required to be on an Inventory Slot with children gameObjects for Name, Amount, and Image
/// Would like to improve this so that it's not an issue, but I don't know how yet
/// </summary>
public class Inventory_Slot : MonoBehaviour
{
    public Item_Stack stk;

    void Start()
    {
        PopulateSlot(stk);
    }
    /// <summary>
    /// Fill the item slot with the given number of items
    /// </summary>
    /// <param name="stack"></param>
    public void PopulateSlot(Item_Stack stack)
    {
        UnityEngine.UI.Image itemImage = GetComponentInChildren<UnityEngine.UI.Image>();
        TextMeshProUGUI name = transform.Find("Name").gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI amount = transform.Find("Amount").gameObject.GetComponent<TextMeshProUGUI>();

        if (itemImage == null || name == null || amount == null)
            Debug.LogError($"[Invtry_Slts] Error: Slot missing a required component");

        // Populate values
        itemImage.sprite = stack.resource.Image;
        name.text = stack.resource.Name;
        amount.text = stack.amount.ToString();



    }
}
