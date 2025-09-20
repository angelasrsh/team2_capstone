using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Choose_Menu_UI : MonoBehaviour
{
    public GameObject menuBox;
    public GameObject darkOverlay;
    public GameObject dayCanvas;
    private GameObject errorText;

    private void Start()
    {
        // Initially show the menu
        menuBox.SetActive(true);
        darkOverlay.SetActive(true);

        errorText = menuBox.transform.Find("Error_Text").gameObject;
        errorText.SetActive(false);

        if (dayCanvas != null)
            dayCanvas.SetActive(false);
    }

    public void ContinueGame()
    {
        if (Choose_Menu_Items.instance.HasSelectedDishes())
        {
            Debug.Log("Continuing to resource gathering...");

            menuBox.SetActive(false);
            darkOverlay.SetActive(false);

            // Fire the event from the persistent script
            Choose_Menu_Items.instance.NotifyMenuConfirmed();

            if (dayCanvas != null)
                dayCanvas.SetActive(true);
            else
                Debug.LogWarning("DayCanvas not found in the scene.");
        }
        else
        {
            Debug.Log("Please select at least one dish to continue.");
            errorText.SetActive(true);
            StartCoroutine(HideMessageAfterDelay(2f));
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        errorText.SetActive(false);
    }

    // Pass-through methods for UI buttons
    public void AddDish(Dish_Data.Dishes dishType) =>
        Choose_Menu_Items.instance.AddDish(dishType);

    public void RemoveDish(Dish_Data.Dishes dishType) =>
        Choose_Menu_Items.instance.RemoveDish(dishType);
}
