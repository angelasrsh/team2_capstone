using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Journal_Menu : MonoBehaviour
{
  public GameObject journal;
  private bool isPaused = false; // Currently will overlap pause menu, I think\
  [SerializeField] private GameObject Dish_Slot_Prefab;
  [SerializeField] private GameObject Dishes_Grid;
  [SerializeField] private GameObject Left_Page;
  [SerializeField] private GameObject Right_Page;
  private Dish_Database dishDatabase;

  private void Start()
  {
    if (journal == null)
      Debug.LogError("Journal_Menu: Journal GameObject not assigned in inspector!");
    else
    {
      journal.transform.GetChild(0).gameObject.SetActive(false);
      journal.transform.GetChild(1).gameObject.SetActive(false);
    }

    if (Dish_Slot_Prefab == null)
      Debug.LogError("Journal_Menu: Dish_Slot_Prefab not assigned in inspector!");

    if (Dishes_Grid == null)
      Debug.LogError("Journal_Menu: Dishes_Grid not assigned in inspector!");

    dishDatabase = Game_Manager.Instance.dishDatabase;
    dishDatabase.OnDishUnlocked += PopulateDishes; // subscribe to the event
    PopulateDishes();
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.J))
    {
      if (isPaused)
        ResumeGame();
      else
        PauseGame();
    }
  }

  public void PauseGame()
  {
    Debug.Log("Opening journal and pausing game...");
    journal.transform.GetChild(0).gameObject.SetActive(true);
    journal.transform.GetChild(1).gameObject.SetActive(true);
    isPaused = true;
  }

  // Resume the game from the pause menu
  public void ResumeGame()
  {
    Debug.Log("Closing journal and resuming game...");
    journal.transform.GetChild(0).gameObject.SetActive(false);
    journal.transform.GetChild(1).gameObject.SetActive(false);
    isPaused = false;
    Left_Page.SetActive(false);
  }

  private void OnDisable()
  {
    dishDatabase.OnDishUnlocked -= PopulateDishes; // unsubscribe to prevent memory leaks
  }

  private void PopulateDishes()
  {
    Debug.Log("Populating dishes in journal...");
    // Clear existing dish slots
    foreach (Transform child in Dishes_Grid.transform)
    {
      DestroyImmediate(child.gameObject, true);
    }

    // Populate with unlocked dishes
    Debug.Log($"Unlocked dishes count: {dishDatabase.GetUnlockedDishes().Count}");
    foreach (var unlockedDishType in dishDatabase.GetUnlockedDishes())
    {
      Dish_Data dishData = dishDatabase.GetDish(unlockedDishType);
      if (dishData == null)
        continue; // safety check

      // Instantiate a dish slot prefab as a child of the grid
      GameObject slot = Instantiate(Dish_Slot_Prefab, Dishes_Grid.transform);

      // Find and set the UI elements within the slot
      Transform dishBG = slot.transform.Find("Dish_BG");
      Transform button = dishBG.Find("Button");
      Transform dishName = button.Find("Dish_Name");
      var textComp = dishName.GetComponent<TextMeshProUGUI>();
      textComp.text = dishData.Name;

      Transform iconPanel = dishBG.Find("Dish_Icon_Panel");
      Transform icon = iconPanel.Find("Dish_Icon");
      var imageComp = icon.GetComponent<UnityEngine.UI.Image>();
      imageComp.sprite = dishData.dishSprite;

      // Add button listener to show details on click
      var buttonComp = button.GetComponent<UnityEngine.UI.Button>();
      buttonComp.onClick.AddListener(() => DisplayDishDetails(dishData));
    }
  }

  private void DisplayDishDetails(Dish_Data dishData)
  {
    Left_Page.SetActive(true);

    // Find left page UI elements
    Transform pagePanel = Left_Page.transform.Find("Page_Panel");
    Transform nameObj = pagePanel.Find("Dish_Name");
    var nameText = nameObj.GetComponent<TextMeshProUGUI>();
    nameText.text = dishData.Name;

    // Find and set the recipe text
    Transform detailsObj = pagePanel.Find("Dish_Details");
    var detailsText = detailsObj.GetComponent<TextMeshProUGUI>();
    detailsText.text = dishData.recipeInstructions;

    // Find and set the dish icon
    Transform iconPanel = pagePanel.Find("Icon_Panel");
    Transform iconObj = iconPanel.Find("Icon_Image");
    var iconImage = iconObj.GetComponent<UnityEngine.UI.Image>();
    iconImage.sprite = dishData.dishSprite;
  }
}
