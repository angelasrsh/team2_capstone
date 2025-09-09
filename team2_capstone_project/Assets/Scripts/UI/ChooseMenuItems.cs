using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseMenuItems : MonoBehaviour
{
  public GameObject menuBox;
  public GameObject darkOverlay;
  private bool selectedDishes;
  private List<string> dishesSelected = new List<string>();
  private GameObject errorText;

  public void Start()
  {
    // Initially show the menu
    menuBox.SetActive(true);
    darkOverlay.SetActive(true);
    selectedDishes = false;
    errorText = menuBox.transform.Find("Error_Text").gameObject;
    errorText.SetActive(false);
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
  public void addDish(string dishName)
  {
    if (!dishesSelected.Contains(dishName))
    {
      dishesSelected.Add(dishName);
      Debug.Log(dishName + " added. Total dishes: " + dishesSelected.Count);
    }
    else
    {
      Debug.Log(dishName + " is already selected.");
    }
  }

  // Remove a dish from the selected list
  public void removeDish(string dishName)
  {
    if (dishesSelected.Contains(dishName))
    {
      dishesSelected.Remove(dishName);
      Debug.Log(dishName + " removed. Total dishes: " + dishesSelected.Count);
    }
    else
    {
      Debug.Log(dishName + " is not in the selected list.");
    }
  }

  // Get the list of selected dishes
  public List<string> getSelectedDishes()
  {
    return dishesSelected;
  }

  // Check if any dishes have been selected
  public bool hasSelectedDishes()
  {
    return selectedDishes;
  }
}