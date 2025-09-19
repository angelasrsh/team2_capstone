using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class Journal_Menu : MonoBehaviour
{
  public GameObject journal;
  private bool isPaused = false; // Currently will overlap pause menu, I think
  private PlayerInput playerInput;
  private InputAction openJournalAction;

  [Header("Recipe Menu References")]
  [SerializeField] private GameObject Dish_Slot_Prefab;
  [SerializeField] private GameObject Dishes_Grid;
  [SerializeField] private GameObject Left_Page;
  [SerializeField] private GameObject Right_Page;

  [Header("Foraging Menu References")]
  [SerializeField] private Choose_Menu_Items dailyMenu;
  [SerializeField] private GameObject Foraging_Slot_Prefab;
  [SerializeField] private GameObject Foraging_Grid;
  [SerializeField] private GameObject Foraging_Left_Page;
  [SerializeField] private GameObject Foraging_Right_Page;

  [Header("Journal Sections")]
  [SerializeField] private CanvasGroup recipeMenuGroup;
  [SerializeField] private CanvasGroup foragingMenuGroup;

  private Dish_Database dishDatabase;
  private Foraging_Database foragingDatabase;

  private void Start()
  {
    PlayerInput playerInput = FindObjectOfType<PlayerInput>();
    if (playerInput == null)
      Debug.LogError("Journal_Menu: No PlayerInput found in scene!");
      
    openJournalAction = playerInput.actions["OpenJournal"];
    if (openJournalAction == null)  
      Debug.LogError("Journal_Menu: 'OpenJournal' action not found in PlayerInput actions!");

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

    if (Foraging_Slot_Prefab == null)
      Debug.LogError("Journal_Menu: Foraging_Slot_Prefab not assigned in inspector!");

    if (Foraging_Grid == null)
      Debug.LogError("Journal_Menu: Foraging_Grid not assigned in inspector!");

    dishDatabase = Game_Manager.Instance.dishDatabase;
    foragingDatabase = Game_Manager.Instance.foragingDatabase;
    if (dishDatabase != null && foragingDatabase != null)
    {
      dishDatabase.OnDishUnlocked += PopulateDishes; // subscribe to the event
      PopulateDishes();
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (openJournalAction.WasPerformedThisFrame())
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
    journal.transform.GetChild(2).gameObject.SetActive(true);
    isPaused = true;

    // Default to showing recipe menu
    ShowGroup(recipeMenuGroup, true);
  }

  // Resume the game from the pause menu
  public void ResumeGame()
  {
    Debug.Log("Closing journal and resuming game...");
    journal.transform.GetChild(0).gameObject.SetActive(false);
    journal.transform.GetChild(1).gameObject.SetActive(false);
    journal.transform.GetChild(2).gameObject.SetActive(false);
    isPaused = false;
    Left_Page.SetActive(false);

    ShowGroup(recipeMenuGroup, false);
    ShowGroup(foragingMenuGroup, false);
  }
  
  private void OnEnable()
  {
      Choose_Menu_Items.OnDailyMenuSelected += PopulateForaging;
  }

  private void OnDisable()
  {
    if (dishDatabase != null)
      dishDatabase.OnDishUnlocked -= PopulateDishes;

    Choose_Menu_Items.OnDailyMenuSelected -= PopulateForaging;
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
          if (dishData == null) continue; // safety check 

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


  private void PopulateForaging(List<Dish_Data.Dishes> selectedDishes)
  {
      Debug.Log("Populating foraging menu...");

      // Clear existing slots
      foreach (Transform child in Foraging_Grid.transform)
      {
          DestroyImmediate(child.gameObject, true);
      }

      // For each selected dish
      foreach (var dishType in selectedDishes)
      {
          Dish_Data dishData = dishDatabase.GetDish(dishType);
          if (dishData == null) continue;

          GameObject dishSlot = Instantiate(Foraging_Slot_Prefab, Foraging_Grid.transform);

          Transform bg = dishSlot.transform.Find("Item_BG");
          Transform button = bg.Find("Item_Button");
          Transform itemName = button.Find("Item_Name");
          var nameText = itemName.GetComponent<TextMeshProUGUI>();
          nameText.text = dishData.Name;

          Transform iconPanel = bg.Find("Item_Icon_Panel");
          Transform icon = iconPanel.Find("Item_Icon");
          var imageComp = icon.GetComponent<UnityEngine.UI.Image>();
          imageComp.sprite = dishData.dishSprite;

          var buttonComp = button.GetComponent<UnityEngine.UI.Button>();
          buttonComp.onClick.AddListener(() => DisplayDishForagingDetails(dishData));
      }
  }

  private void DisplayDishForagingDetails(Dish_Data dishData)
  {
      Foraging_Left_Page.SetActive(true);

      Transform pagePanel = Foraging_Left_Page.transform.Find("Left_Page_Item_Panel");

      // Dish Info
      pagePanel.Find("Item_Name").GetComponent<TextMeshProUGUI>().text = dishData.Name;
      pagePanel.Find("Item_Icon_Panel/Item_Image").GetComponent<UnityEngine.UI.Image>().sprite = dishData.dishSprite;

      // Collect ingredient lines
      string ingredientText = "";
      foreach (var req in dishData.ingredientQuantities)
      {
          Ingredient_Data ingredient = req.ingredient;
          int required = req.amountRequired;
          
          // Pick the display ingredient
          Ingredient_Data displayIngredient = ingredient;

          // If this ingredient has equivalencies, pick the first one as the base
          if (ingredient.countsAs != null && ingredient.countsAs.Count > 0)
          {
              displayIngredient = ingredient.countsAs[0];
          }

          // Now display the base ingredient instead of the processed one
          ingredientText += $"{displayIngredient.Name} x{required}\n";

      }

      // Assign once
      var detailsText = pagePanel.Find("Item_Details").GetComponent<TextMeshProUGUI>();
      detailsText.text = ingredientText.TrimEnd(); // trim last newline
  }


  private void ShowGroup(CanvasGroup group, bool show)
  {
    group.alpha = show ? 1 : 0;             
    group.interactable = show;         
    group.blocksRaycasts = show;            
  }

  public void ShowRecipeMenu()
  {
    ShowGroup(recipeMenuGroup, true);
    ShowGroup(foragingMenuGroup, false);
  }

  public void ShowForagingMenu()
  {
    ShowGroup(recipeMenuGroup, false);
    ShowGroup(foragingMenuGroup, true);
  }
}
