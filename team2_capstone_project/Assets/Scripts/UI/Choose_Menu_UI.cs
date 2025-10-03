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
        // Show the menu screen on day start
        menuBox.SetActive(true);
        darkOverlay.SetActive(true);

        errorText = menuBox.transform.Find("Error_Text").gameObject;
        errorText.SetActive(false);

        if (dayCanvas != null)
            dayCanvas.SetActive(false);

        // build UI from daily pool
        PopulateMenuOptions();
    }

    private void OnEnable()
    {
        Day_Turnover_Manager.OnDayEnded += HandleDayEnded;
    }

    private void OnDisable()
    {
        Day_Turnover_Manager.OnDayEnded -= HandleDayEnded;
    }

    private void HandleDayEnded(Day_Summary_Data summary)
    {
        // For each new day, reopen menu UI and rebuild options
        menuBox.SetActive(true);
        darkOverlay.SetActive(true);
        if (dayCanvas != null) dayCanvas.SetActive(false);

        PopulateMenuOptions();
    }

    private void PopulateMenuOptions()
    {
        var pool = Choose_Menu_Items.instance.GetDailyPool();
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("No dishes in the daily pool!");
            return;
        }

        // Find container for dish options
        Transform dishContainer = menuBox.transform.Find("Dish_Options");
        if (dishContainer == null)
        {
            Debug.LogWarning("Dish_Options container not found in UI!");
            return;
        }

        // Clear old options
        foreach (Transform child in dishContainer)
            Destroy(child.gameObject);

        // Spawn buttons for each dish in the pool
        foreach (var dishType in pool)
        {
            var dish = Game_Manager.Instance.dishDatabase.GetDish(dishType);
            if (dish == null) continue;

            GameObject button = Instantiate(Resources.Load<GameObject>("DishButtonPrefab"), dishContainer);
            button.GetComponentInChildren<UnityEngine.UI.Text>().text = dish.name;

            // Assign click listener
            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                AddDish(dish.dishType);
            });
        }
    }

    public void ContinueGame()
    {
        if (Choose_Menu_Items.instance.HasSelectedDishes())
        {
            Debug.Log("Continuing to resource gathering...");

            menuBox.SetActive(false);
            darkOverlay.SetActive(false);

            // Notify that menu selection is confirmed in persistent manager
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
   public void AddDish(Dish_Data.Dishes dishType)
    {
        if (Choose_Menu_Items.instance.GetSelectedDishes().Count >= 2)
        {
            Debug.Log("You can only select up to 2 dishes!");
            errorText.SetActive(true);
            StartCoroutine(HideMessageAfterDelay(2f));
            return;
        }

        Choose_Menu_Items.instance.AddDish(dishType);
    }

    public void RemoveDish(Dish_Data.Dishes dishType) =>
        Choose_Menu_Items.instance.RemoveDish(dishType);
}
