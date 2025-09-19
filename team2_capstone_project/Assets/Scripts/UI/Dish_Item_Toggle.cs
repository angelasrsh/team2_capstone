using Grimoire;
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
          if (System.Enum.TryParse(gameObject.name, out Dish_Data.Dishes dishEnum))
          {
              if (isOn)
              {
                  menu.AddDish(dishEnum);
                  Audio_Manager.instance.PlaySFX(Audio_Manager.instance.clickSFX);
              }
              else
              {
                  menu.RemoveDish(dishEnum);
                  Audio_Manager.instance.PlaySFX(Audio_Manager.instance.clickSFX);
              }
          }
          else
          {
              Debug.LogWarning($"GameObject name {gameObject.name} did not match Dish_Data.Dishes enum.");
          }
      }
      else
      {
          Debug.LogWarning("Choose_Menu_Items script not found in the scene.");
      }
  }
}
