using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Choose_Menu_UI : MonoBehaviour
{
    [Header("Active UI")]
    public GameObject menuBox;
    public GameObject darkOverlay;
    public GameObject dayCanvas;
    private GameObject errorText;

    [Header("Prefabs")]
    public GameObject Dish_Slot_Prefab;

    private Day_Turnover_Manager.TimeOfDay timeOfDay;

    private void Start()
    {
        if (Day_Turnover_Manager.Instance != null && 
            Day_Turnover_Manager.Instance.currentTimeOfDay == Day_Turnover_Manager.TimeOfDay.Morning)
        {
            menuBox.SetActive(true);
            darkOverlay.SetActive(true);

            errorText = menuBox.transform.Find("Error_Text").gameObject;
            errorText.SetActive(false);

            if (dayCanvas != null)
                dayCanvas.SetActive(false);

            // build UI from daily pool
            PopulateMenuOptions();
        }
        else
        {
            // Ensure it's hidden outside morning
            menuBox.SetActive(false);
            darkOverlay.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Day_Turnover_Manager.OnDayStarted += HandleDayStarted;
    }

    private void OnDisable()
    {
        Day_Turnover_Manager.OnDayStarted -= HandleDayStarted;
    }

    private void HandleDayStarted()
    {
        if (Day_Turnover_Manager.Instance.currentTimeOfDay == Day_Turnover_Manager.TimeOfDay.Morning)
        {
            menuBox.SetActive(true);
            darkOverlay.SetActive(true);
            if (dayCanvas != null) dayCanvas.SetActive(false);

            PopulateMenuOptions();
        }
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
        Transform dishContainer = menuBox.transform.Find("Dish_Container");
        if (dishContainer == null)
        {
            Debug.LogWarning("Dish_Container not found in UI!");
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

            GameObject button = Instantiate(Dish_Slot_Prefab, dishContainer.transform);

            // Set UI
            var textComp = button.transform.Find("Content/Dish_Name").GetComponent<TextMeshProUGUI>();
            textComp.text = dish.Name;

            var imageComp = button.transform.Find("Content/Dish_Icon").GetComponent<Image>();
            imageComp.sprite = dish.Image;


            // Inject dish data into toggle
            var toggleComp = button.GetComponent<Dish_Item_Toggle>();
            if (toggleComp != null)
                toggleComp.Initialize(dish.dishType);
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
