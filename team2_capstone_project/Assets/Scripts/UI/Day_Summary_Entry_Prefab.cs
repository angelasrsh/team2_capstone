using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Day_Summary_Entry_Prefab : MonoBehaviour
{
    [Header("References")]
    public Image background; 
    public Image icon;  // Dish/customer sprite
    public TextMeshProUGUI label;
    public TextMeshProUGUI quantity;

    public void Setup(Sprite iconSprite, string labelText, int qty)
    {
        // Label
        if (label != null) label.text = labelText;
        if (quantity != null) quantity.text = $"x{qty}";

        // Icon
        if (icon != null)
        {
            if (iconSprite != null)
            {
                icon.sprite = iconSprite;
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false; // hides filler when no sprite
            }
        }
    }
}
