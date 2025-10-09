using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Grimoire;
using System.Linq;

public class Journal_Menu : MonoBehaviour
{
  public GameObject journal;
  private bool isPaused = false; // Currently will overlap pause menu, I think
  private Player_Progress playerProgress = Player_Progress.Instance;
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

  [Header("NPC Menu References")]
  [SerializeField] private GameObject NPC_Slot_Prefab;
  [SerializeField] private GameObject NPC_Grid;
  [SerializeField] private GameObject NPC_Left_Page;
  [SerializeField] private GameObject NPC_Right_Page;
  [SerializeField] private Slider NPC_Slider;

  [Header("Journal Sections")]
  [SerializeField] private CanvasGroup recipeMenuGroup;
  [SerializeField] private CanvasGroup foragingMenuGroup;
  [SerializeField] private CanvasGroup npcMenuGroup;

  private Dish_Database dishDatabase;
  private Foraging_Database foragingDatabase;

  // private void Awake()
  // {
  //     dishDatabase = Game_Manager.Instance.dishDatabase;
  //     foragingDatabase = Game_Manager.Instance.foragingDatabase;
  // }

  private void Start()
  {
    Player_Input_Controller pic = FindObjectOfType<Player_Input_Controller>();
    if (pic != null)
    {
        openJournalAction = pic.GetComponent<PlayerInput>().actions["OpenJournal"];
    }

    if (journal == null)
      Debug.LogError("Journal_Menu: Journal GameObject not assigned in inspector!");
    else
    {
      HideEverything();
    }

    if (Dish_Slot_Prefab == null)
      Debug.LogError("Journal_Menu: Dish_Slot_Prefab not assigned in inspector!");

    if (Dishes_Grid == null)
      Debug.LogError("Journal_Menu: Dishes_Grid not assigned in inspector!");

    if (Foraging_Slot_Prefab == null)
      Debug.LogError("Journal_Menu: Foraging_Slot_Prefab not assigned in inspector!");

    if (Foraging_Grid == null)
      Debug.LogError("Journal_Menu: Foraging_Grid not assigned in inspector!");

    if (Choose_Menu_Items.instance == null)
      Debug.LogWarning("Choose_Menu_Items instance is NULL in this scene!");
    else
      Debug.Log("Choose_Menu_Items still alive. Dishes count: " + Choose_Menu_Items.instance.GetSelectedDishes().Count);

    PopulateDishes();
    PopulateNPCs();
  }

  // Update is called once per frame
  void Update()
  {
    if (openJournalAction.WasPerformedThisFrame())
    {
      if (isPaused)
      {
          ResumeGame();
          Game_Events_Manager.Instance.JournalToggled(false);
      }
      else
      {
          PauseGame();
          Game_Events_Manager.Instance.JournalToggled(true);
      }
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

    Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.bookClose);

    journal.transform.GetChild(0).gameObject.SetActive(false);
    journal.transform.GetChild(1).gameObject.SetActive(false);
    journal.transform.GetChild(2).gameObject.SetActive(false);
    isPaused = false;
    Left_Page.SetActive(false);

    ShowGroup(recipeMenuGroup, false);
    ShowGroup(foragingMenuGroup, false);
    ShowGroup(npcMenuGroup, false);
  }

  private void OnEnable()
 {
    dishDatabase = Game_Manager.Instance.dishDatabase;
    foragingDatabase = Game_Manager.Instance.foragingDatabase;
    
    Choose_Menu_Items.OnDailyMenuSelected += PopulateForaging;
    Debug.Log("Subscribed to OnDailyMenuSelected event.");

    if (Choose_Menu_Items.instance != null && Choose_Menu_Items.instance.HasSelectedDishes() && dishDatabase != null)
    {
      PopulateForaging(Choose_Menu_Items.instance.GetSelectedDishes());
      Debug.Log("Populated foraging menu from existing daily menu selection.");
    }

    if (Player_Progress.Instance != null)
    {
      playerProgress.OnDishUnlocked += PopulateDishes;
      playerProgress.OnDishUnlocked += PopulateNPCs;
    }
  }

  private void OnDisable()
  {
    if (Player_Progress.Instance != null)
    {
      Player_Progress.Instance.OnDishUnlocked -= PopulateDishes;
      Player_Progress.Instance.OnDishUnlocked -= PopulateNPCs;
    }

    Choose_Menu_Items.OnDailyMenuSelected -= PopulateForaging;
  }

  #region Recipe Menu Methods
  private void PopulateDishes()
  {
    Debug.Log("Populating dishes in journal...");

    // Clear existing dish slots
    foreach (Transform child in Dishes_Grid.transform)
    {
      DestroyImmediate(child.gameObject, true);
    }

    // Populate with unlocked dishes
    Debug.Log($"Unlocked dishes count: {playerProgress.GetUnlockedDishes().Count}");
    foreach (var unlockedDishType in playerProgress.GetUnlockedDishes())
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
      imageComp.sprite = dishData.Image;
      
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
        Transform recipeObj = pagePanel.Find("Recipe_Image");
        Image recipeImage = recipeObj.GetComponent<Image>();
        recipeImage.sprite = dishData.recipeImage;
        recipeImage.preserveAspect = true;

        // Find and set the dish icon
        Transform iconPanel = pagePanel.Find("Icon_Panel");
        Transform iconObj = iconPanel.Find("Icon_Image");
        Image iconImage = iconObj.GetComponent<UnityEngine.UI.Image>();
        iconImage.sprite = dishData.Image;
    
        // Broadcast event for tutorial
        Game_Events_Manager.Instance.DishDetailsClick(dishData);
  }
  #endregion

  #region Daily Menu Methods
  private void PopulateForaging(List<Dish_Data.Dishes> selectedDishes)
  {
    Debug.Log("Populating foraging menu...");

    // Clear existing slots
    foreach (Transform child in Foraging_Grid.transform)
    {
      DestroyImmediate(child.gameObject, true);
    }

    // For each selected dish
   Debug.Log("PopulateForaging called. playerProgress: " + (playerProgress != null) +
          ", ForagingPrefab: " + (Foraging_Slot_Prefab != null) +
          ", ContentParent: " + (Foraging_Grid != null));

    foreach (var dishType in selectedDishes)
    {
      Debug.Log("Trying to fetch dish: " + dishType);
      var dishData = dishDatabase.GetDish(dishType); // line 215?
      if (dishData == null)
      {
          Debug.LogError("Dish not found in database: " + dishType);
          continue;
      }
      GameObject dishSlot = Instantiate(Foraging_Slot_Prefab, Foraging_Grid.transform);

      Transform bg = dishSlot.transform.Find("Item_BG");
      Transform button = bg.Find("Item_Button");
      Transform itemName = button.Find("Item_Name");
      var nameText = itemName.GetComponent<TextMeshProUGUI>();
      nameText.text = dishData.Name;

      Transform iconPanel = bg.Find("Item_Icon_Panel");
      Transform icon = iconPanel.Find("Item_Icon");
      var imageComp = icon.GetComponent<UnityEngine.UI.Image>();
      imageComp.sprite = dishData.Image;

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
    pagePanel.Find("Item_Icon_Panel/Item_Image").GetComponent<UnityEngine.UI.Image>().sprite = dishData.Image;

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
  #endregion

  #region NPC Menu Methods
  private void PopulateNPCs()
  {
    Debug.Log("Populating npcs in journal...");

    // Clear existing npc slots
    foreach (Transform child in NPC_Grid.transform)
    {
      DestroyImmediate(child.gameObject, true);
    }

    // Populate with unlocked dishes
    Debug.Log($"Unlocked npcs count: {playerProgress.GetUnlockedNPCs().Count}");
    foreach (var unlockedNPC in playerProgress.GetUnlockedNPCs())
    {
      CustomerData npcData = NPC_Database.Instance.GetNPCData(unlockedNPC);
      if (npcData == null) continue; // safety check 

      // Instantiate an npc slot prefab as a child of the grid
      GameObject slot = Instantiate(NPC_Slot_Prefab, NPC_Grid.transform);

      // Find and set the UI elements within the slot
      Transform NPCBG = slot.transform.Find("NPC_Button_BG");
      Transform button = NPCBG.Find("Button");
      Transform npcName = button.Find("NPC_Name");
      var textComp = npcName.GetComponent<TextMeshProUGUI>();
      textComp.text = npcData.customerName;

      Transform iconPanel = NPCBG.Find("Icon_Panel");
      Transform icon = iconPanel.Find("Icon");
      var imageComp = icon.GetComponent<UnityEngine.UI.Image>();
      imageComp.sprite = npcData.defaultPortrait;

      // Add button listener to show details on click
      var buttonComp = button.GetComponent<UnityEngine.UI.Button>();
      buttonComp.onClick.AddListener(() => DisplayNPCDetails(npcData));
    }
  }
    private void DisplayNPCDetails(CustomerData npcData)
    {
        NPC_Left_Page.SetActive(true);

        Transform pagePanel = NPC_Left_Page.transform.Find("Left_Page_Panel");

        // NPC Info
        pagePanel.Find("NPC_Name").GetComponent<TextMeshProUGUI>().text = npcData.customerName;
        pagePanel.Find("NPC_Icon_Panel/NPC_Image").GetComponent<UnityEngine.UI.Image>().sprite = npcData.defaultPortrait;

        // Build the description text
        string npcText = "Background Info: \n" + npcData.lore + "\n\n";

        npcText += "Favorite Dishes: " +
            (npcData.favoriteDishes.Length > 0
                ? string.Join(", ", npcData.favoriteDishes.Select(d => d.Name))
                : "None") + "\n";

        npcText += "Disliked Dishes: " +
            (npcData.dislikedDishes.Length > 0
                ? string.Join(", ", npcData.dislikedDishes.Select(d => d.Name))
                : "None") + "\n";

        npcText += "Neutral Dishes: " +
            (npcData.neutralDishes.Length > 0
                ? string.Join(", ", npcData.neutralDishes.Select(d => d.Name))
                : "None");

        // Assign to text component
        var detailsText = pagePanel.Find("NPC_Details").GetComponent<TextMeshProUGUI>();
        detailsText.text = npcText;

        // Assign slider value
        NPC_Slider.value = Affection_System.Instance.GetAffectionLevel(npcData);
  }
  #endregion

  #region Show/Hide UI Helper Methods
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
    ShowGroup(npcMenuGroup, false);
  }

  public void ShowForagingMenu()
  {
    ShowGroup(recipeMenuGroup, false);
    ShowGroup(foragingMenuGroup, true);
    ShowGroup(npcMenuGroup, false);
  }

  public void ShowNPCMenu()
  {
    ShowGroup(npcMenuGroup, true);
    ShowGroup(foragingMenuGroup, false);
    ShowGroup(recipeMenuGroup, false);
  }

  public void HideEverything()
  {
    journal.transform.GetChild(0).gameObject.SetActive(false);
    journal.transform.GetChild(1).gameObject.SetActive(false);
    journal.transform.GetChild(2).gameObject.SetActive(false);
    ShowGroup(recipeMenuGroup, false);
    ShowGroup(foragingMenuGroup, false);
    ShowGroup(npcMenuGroup, false);
  }
  #endregion
}
