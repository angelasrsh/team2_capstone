using UnityEngine;
using UnityEngine.UI;

public class DishItemToggle : MonoBehaviour
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

  // Add or remove dish from the selected list in ChooseMenuItems
  private void addOrRemoveDish(bool isOn)
  {
    ChooseMenuItems menu = FindObjectOfType<ChooseMenuItems>();
    if (menu != null)
    {
      string dishName = gameObject.name; // Assuming the GameObject's name is the dish name
      if (isOn)
      {
        menu.addDish(dishName);
      }
      else
      {
        menu.removeDish(dishName);
      }
    }
    else
    {
      Debug.LogWarning("ChooseMenuItems script not found in the scene.");
    }
  }
}
