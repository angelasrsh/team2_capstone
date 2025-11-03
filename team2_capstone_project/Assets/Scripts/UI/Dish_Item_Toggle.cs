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

  private Dish_Data.Dishes dishType; // assigned at spawn time

  private void Start()
  {
    if (toggle != null)
      toggle.onValueChanged.AddListener(OnToggleChanged);
  }

  public void Initialize(Dish_Data.Dishes dish)
  {
    dishType = dish;
    gameObject.name = dish.ToString();
  }

  private void OnToggleChanged(bool isOn)
  {
      if (toggle == null) return;

      var menu = Choose_Menu_Items.instance;
      if (menu == null)
      {
          Debug.LogWarning("Choose_Menu_Items instance not found in the scene.");
          return;
      }

      if (isOn)
      {
          bool added = menu.AddDish(dishType);
          if (!added)
          {
              // ❌ If adding failed, revert the toggle
              toggle.isOn = false;

              // Optionally trigger UI feedback if available
              var ui = FindObjectOfType<Choose_Menu_UI>();
              if (ui != null)
                  ui.ShowSelectionError($"You can only select up to {menu.maxSelect} dishes!");

              return;
          }
      }
      else
          menu.RemoveDish(dishType);

      UpdateColor(isOn);
      Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.clickSFX, 0.5f, 1f);
  }

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
