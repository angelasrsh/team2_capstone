using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Choose_Menu_Items : MonoBehaviour
{
  public GameObject menuBox;
  public GameObject darkOverlay;
  public GameObject dayCanvas;
  private bool selectedDishes;
  private GameObject errorText;
  
  private List<Dish_Data.Dishes> dishesSelected = new List<Dish_Data.Dishes>();
  public static event System.Action<List<Dish_Data.Dishes>> OnDailyMenuSelected;

  public void Start()
  {
    // Initially show the menu
    menuBox.SetActive(true);
    darkOverlay.SetActive(true);
    selectedDishes = false;
    errorText = menuBox.transform.Find("Error_Text").gameObject;
    errorText.SetActive(false);
    dayCanvas.SetActive(false);
  }

  // Resume the game from the pause menu
  public void ContinueGame()
  {
    if (dishesSelected.Count > 0)
    {
      Debug.Log("Continuing to resource gathering...");
      menuBox.SetActive(false);
      darkOverlay.SetActive(false);
      selectedDishes = true;
      OnDailyMenuSelected?.Invoke(dishesSelected);

      // Activate Day Canvas
      if (dayCanvas != null)
      {
        dayCanvas.SetActive(true);
      }
      else
      {
        Debug.LogWarning("DayCanvas not found in the scene.");
      }
    }
    else
    {
      Debug.Log("Please select at least one dish to continue.");
      errorText.SetActive(true);
      StartCoroutine(HideMessageAfterDelay(2f));
    }
  }

  // Coroutine to hide the error message after a delay
  private IEnumerator HideMessageAfterDelay(float delay)
  {
    yield return new WaitForSeconds(delay);
    errorText.SetActive(false);
  }

  // Add a dish to the selected list
  public void AddDish(Dish_Data.Dishes dishType)
  {
      if (!dishesSelected.Contains(dishType))
      {
          dishesSelected.Add(dishType);
          Debug.Log(dishType + " added. Total dishes: " + dishesSelected.Count);
      }
      else
      {
          Debug.Log(dishType + " is already selected.");
      }
  }

  public void RemoveDish(Dish_Data.Dishes dishType)
  {
      if (dishesSelected.Contains(dishType))
      {
          dishesSelected.Remove(dishType);
          Debug.Log(dishType + " removed. Total dishes: " + dishesSelected.Count);
      }
      else
      {
          Debug.Log(dishType + " is not in the selected list.");
      }
  }

  // Get the list of selected dishes
  public List<Dish_Data.Dishes> GetSelectedDishes()
  {
      return dishesSelected;
  }

  // Check if any dishes have been selected
  public bool HasSelectedDishes()
  {
    return selectedDishes;
  }
}