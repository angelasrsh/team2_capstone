using UnityEngine;
using UnityEngine.UI;

public class Dish_Item_Toggle : MonoBehaviour
{
  public Toggle toggle;

  // Toggle colors
  public Color onColor = Color.red;
  public Color offColor = Color.white;

  public Graphic targetGraphic;

  void Start()
  {
    toggle.onValueChanged.AddListener(OnToggleChanged);
  }

  private void OnToggleChanged(bool isOn)
  {
    Debug.Log("toggle changed to: " + isOn);
    UpdateColor(isOn);
    addOrRemoveDish(isOn);
  }

  // Update the color based on the toggle state
  private void UpdateColor(bool isOn)
  {
    if (targetGraphic != null)
      targetGraphic.color = isOn ? onColor : offColor;
  }

  // Add or remove dish from the selected list in Choose_Menu_Items
  private void addOrRemoveDish(bool isOn)
  {
    Choose_Menu_Items menu = FindObjectOfType<Choose_Menu_Items>();
    if (menu != null)
    {
      string dishName = gameObject.name; // Assuming the GameObject's name is the dish name
      if (isOn)
      {
        menu.AddDish(dishName);
      }
      else
      {
        menu.RemoveDish(dishName);
      }
    }
    else
    {
      Debug.LogWarning("Choose_Menu_Items script not found in the scene.");
    }
  }
}
