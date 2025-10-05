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
  public static Journal_Menu Instance;
  private bool isPaused = false; // Currently will overlap pause menu, I think
  private Player_Progress playerProgress = Player_Progress.Instance;
  private InputAction openJournalAction;
  private Dish_Database dishDatabase;
  private Foraging_Database foragingDatabase;
  private NPC_Database npcDatabase;
  private List<Dish_Data> allDishes;
  private List<Ingredient_Data> allIngredients;
  private List<CustomerData> allNPCs;
  private GameObject darkOverlay; // journal's dark overlay child
  private GameObject journalContents; // journal's background child
  private GameObject tabs; // journal's tab child
  private int objectsPerPage = 6; // right page only fits 6 objects per page
  private Choose_Menu_Items dailyMenu;
  [Header("Left Page References")]
  [SerializeField] private TextMeshProUGUI detailsText;
  [SerializeField] private TextMeshProUGUI nameText;
  [SerializeField] private Image recipeImage;
  [SerializeField] private Image icon;
  [SerializeField] private GameObject leftPagePanel; // used only to turn it off
  [SerializeField] private Slider affectionGauge;
  [Header("Right Page References")]
  [SerializeField] public List<JournalSlot> slots; // assign 6 slots in inspector
  
  [SerializeField] private Transform objectGrid; // Right page's grid of buttons

  private Tabs currentTab = Tabs.Dish;
  // Dictionary allows players to open to last page they had open in each tab
  private Dictionary<Tabs, int> tabCurrentPage = new Dictionary<Tabs, int>()
  {
    { Tabs.Dish, 0 },
    { Tabs.Ingredient, 0 },
    { Tabs.NPC, 0 }
  };
  private Dictionary<Tabs, int> tabMaxPages;
  public enum Tabs
  {
    Dish,
    Ingredient,
    NPC
  }

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    if (scene.name == "MainMenu")
      gameObject.SetActive(false);
    else
      gameObject.SetActive(true);
  }

  private void Start()
  {
    dishDatabase = Game_Manager.Instance.dishDatabase;
    foragingDatabase = Game_Manager.Instance.foragingDatabase;
    npcDatabase = Game_Manager.Instance.npcDatabase;

    allDishes = Game_Manager.Instance.dishDatabase.dishes;
    allIngredients = Game_Manager.Instance.foragingDatabase.foragingItems;
    // If wanting to use all ingredients list instead of showing only the raw foragable ones:
    // allIngredients = Ingredient_Inventory.Instance.AllIngredientList;
    allNPCs = Game_Manager.Instance.npcDatabase.allNPCs;

    tabMaxPages = new Dictionary<Tabs, int>()
    {
      { Tabs.Dish, Mathf.CeilToInt((float)allDishes.Count / objectsPerPage) },
      { Tabs.Ingredient, Mathf.CeilToInt((float)allIngredients.Count / objectsPerPage) },
      { Tabs.NPC, Mathf.CeilToInt((float)allNPCs.Count / objectsPerPage) }
    };

    darkOverlay = transform.GetChild(0).gameObject;
    journalContents = transform.GetChild(1).gameObject;
    tabs = transform.GetChild(2).gameObject;

    Player_Input_Controller pic = FindObjectOfType<Player_Input_Controller>();
    if (pic != null)
    {
      openJournalAction = pic.GetComponent<PlayerInput>().actions["OpenJournal"];
    }

    if (detailsText == null)
      Debug.LogError("[Journal_Menu]: detailsText not assigned in inspector!");

    if (recipeImage == null)
      Debug.LogError("[Journal_Menu]: recipeImage not assigned in inspector!");

    if (affectionGauge == null)
      Debug.LogError("[Journal_Menu]: affectionGauge not assigned in inspector!");

    if (Choose_Menu_Items.instance == null)
      Debug.LogWarning("Choose_Menu_Items instance is NULL in this scene!");
    else
      Debug.Log("Choose_Menu_Items still alive. Dishes count: " + Choose_Menu_Items.instance.GetSelectedDishes().Count);

    // if (Player_Progress.Instance != null)
    // {
    //   playerProgress.OnDishUnlocked += PopulateDishes;
    //   playerProgress.OnIngredientUnlocked += PopulateIngredients;
    //   playerProgress.OnNPCUnlocked += PopulateNPCs;
    // }

    // PopulateDishes();
    // PopulateNPCs();
    // PopulateIngredients();
  }

  private void OnDestroy()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
    // if (Player_Progress.Instance != null)
    // {
    //   Player_Progress.Instance.OnDishUnlocked -= PopulateDishes;
    //   Player_Progress.Instance.OnDishUnlocked -= PopulateNPCs;
    // }
  }

  // Update is called once per frame
  void Update()
  {
    if (openJournalAction.WasPerformedThisFrame())
    {
      if (isPaused)
      {
        ResumeGame();
        Game_Events_Manager.Instance.JournalToggled(false); //this is being checked every time the journal is opened/closed. Might not be ideal
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
    darkOverlay.SetActive(true);
    journalContents.SetActive(true);
    tabs.SetActive(true);
    isPaused = true;
  }

  public void ResumeGame()
  {
    Debug.Log("Closing journal and resuming game...");

    Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.bookClose);

    darkOverlay.SetActive(false);
    journalContents.SetActive(false);
    tabs.SetActive(false);
    isPaused = false;
  }

  #region Recipe Menu Methods
  private void PopulateDishes(int currentPage)
  {
    Debug.Log("Populating dishes for current page in journal...");

    List<Dish_Data.Dishes> unlockedDishes = playerProgress.GetUnlockedDishes();
    int currGridCell = 0;
    int currUnlockedIndex = (currentPage - 1) * objectsPerPage;
    Transform dishBG = null;
    Transform button = null;
    Transform dishName = null;
    TextMeshProUGUI textComp = null;
    Transform iconPanel = null;
    Transform icon = null;
    UnityEngine.UI.Image imageComp = null;
    UnityEngine.UI.Button buttonComp = null;

    // Populate with unlocked dishes if needed
    while (currGridCell < 6)
    {
      GameObject slot = objectGrid.GetChild(currGridCell).gameObject;
      if (currUnlockedIndex < unlockedDishes.Count)
      {
        Dish_Data dishData = dishDatabase.GetDish(unlockedDishes[currUnlockedIndex]);

        // Find and set the UI elements within the slot
        dishBG = slot.transform.Find("Background");
        button = dishBG.Find("Button");
        dishName = button.Find("Name");
        textComp = dishName.GetComponent<TextMeshProUGUI>();
        textComp.text = dishData.Name;

        iconPanel = dishBG.Find("Icon_Panel");
        icon = iconPanel.Find("Icon");
        imageComp = icon.GetComponent<UnityEngine.UI.Image>();
        imageComp.sprite = dishData.Image;

        // Add button listener to show details on click
        buttonComp = button.GetComponent<UnityEngine.UI.Button>();
        buttonComp.onClick.AddListener(() => DisplayDishDetails(dishData));

        currUnlockedIndex++;
      }
      // ADD IN CODE FOR THE LOCKED DISHES INFO STUFF AND REPLACE ^ WITH JOURNALSLOT METHOD CALL WHATEVER
      currGridCell++;
    }
  }

  private void DisplayDishDetails(Dish_Data dishData)
  {
    leftPagePanel.SetActive(true);

    nameText.text = dishData.Name;
    recipeImage.sprite = dishData.recipeImage;
    recipeImage.preserveAspect = true;
    icon.sprite = dishData.Image;
    icon.preserveAspect = true;
  }
  #endregion

  #region Daily Menu Methods
  private void PopulateIngredients(int currentPage)
  {
    //   Debug.Log("Populating foraging menu...");

    //   // Clear existing slots
    //   foreach (Transform child in Foraging_Grid.transform)
    //   {
    //     DestroyImmediate(child.gameObject, true);
    //   }

    //   // For each selected dish
    //  Debug.Log("PopulateForaging called. playerProgress: " + (playerProgress != null) +
    //         ", ForagingPrefab: " + (Foraging_Slot_Prefab != null) +
    //         ", ContentParent: " + (Foraging_Grid != null));

    //   foreach (var dishType in selectedDishes)
    //   {
    //     Debug.Log("Trying to fetch dish: " + dishType);
    //     var dishData = dishDatabase.GetDish(dishType); // line 215?
    //     if (dishData == null)
    //     {
    //         Debug.LogError("Dish not found in database: " + dishType);
    //         continue;
    //     }
    //     GameObject dishSlot = Instantiate(Foraging_Slot_Prefab, Foraging_Grid.transform);

    //     Transform bg = dishSlot.transform.Find("Item_BG");
    //     Transform button = bg.Find("Item_Button");
    //     Transform itemName = button.Find("Item_Name");
    //     var nameText = itemName.GetComponent<TextMeshProUGUI>();
    //     nameText.text = dishData.Name;

    //     Transform iconPanel = bg.Find("Item_Icon_Panel");
    //     Transform icon = iconPanel.Find("Item_Icon");
    //     var imageComp = icon.GetComponent<UnityEngine.UI.Image>();
    //     imageComp.sprite = dishData.Image;

    //     var buttonComp = button.GetComponent<UnityEngine.UI.Button>();
    //     buttonComp.onClick.AddListener(() => DisplayDishForagingDetails(dishData));
    //   }
  }

  private void DisplayIngredientDetails(Ingredient_Data ingredient)
  {
    leftPagePanel.SetActive(true);

    // Dish Info
    nameText.text = ingredient.Name;
    icon.sprite = ingredient.Image;
    icon.preserveAspect = true;

    // Collect ingredient lines
    //   string ingredientText = "";
    //   foreach (var req in dishData.ingredientQuantities)
    //   {
    //     Ingredient_Data ingredient = req.ingredient;
    //     int required = req.amountRequired;

    //     // Pick the display ingredient
    //     Ingredient_Data displayIngredient = ingredient;

    //     // If this ingredient has equivalencies, pick the first one as the base
    //     if (ingredient.countsAs != null && ingredient.countsAs.Count > 0)
    //     {
    //       displayIngredient = ingredient.countsAs[0];
    //     }

    //     // Now display the base ingredient instead of the processed one
    //     ingredientText += $"{displayIngredient.Name} x{required}\n";

    //   }

    //   // Assign once
    //   var detailsText = pagePanel.Find("Item_Details").GetComponent<TextMeshProUGUI>();
    //   detailsText.text = ingredientText.TrimEnd(); // trim last newline

    //   // Broadcast event for tutorial
    //   Game_Events_Manager.Instance.ForageDetailsClick();
  }
  #endregion

  #region NPC Menu Methods
  private void PopulateNPCs(int currentPage)
  {
    //   Debug.Log("Populating npcs in journal...");

    //   // Clear existing npc slots
    //   foreach (Transform child in NPC_Grid.transform)
    //   {
    //     DestroyImmediate(child.gameObject, true);
    //   }

    //   // Populate with unlocked dishes
    //   Debug.Log($"Unlocked npcs count: {playerProgress.GetUnlockedNPCs().Count}");
    //   foreach (var unlockedNPC in playerProgress.GetUnlockedNPCs())
    //   {
    //     CustomerData npcData = NPC_Database.Instance.GetNPCData(unlockedNPC);
    //     if (npcData == null) continue; // safety check 

    //     // Instantiate an npc slot prefab as a child of the grid
    //     GameObject slot = Instantiate(NPC_Slot_Prefab, NPC_Grid.transform);

    //     // Find and set the UI elements within the slot
    //     Transform NPCBG = slot.transform.Find("NPC_Button_BG");
    //     Transform button = NPCBG.Find("Button");
    //     Transform npcName = button.Find("NPC_Name");
    //     var textComp = npcName.GetComponent<TextMeshProUGUI>();
    //     textComp.text = npcData.customerName;

    //     Transform iconPanel = NPCBG.Find("Icon_Panel");
    //     Transform icon = iconPanel.Find("Icon");
    //     var imageComp = icon.GetComponent<UnityEngine.UI.Image>();
    //     imageComp.sprite = npcData.defaultPortrait;

    //     // Add button listener to show details on click
    //     var buttonComp = button.GetComponent<UnityEngine.UI.Button>();
    //     buttonComp.onClick.AddListener(() => DisplayNPCDetails(npcData));
    //   }
  }
  private void DisplayNPCDetails(CustomerData npcData)
  {
    leftPagePanel.SetActive(true);

    // NPC Info
    nameText.text = npcData.customerName;
    icon.sprite = npcData.defaultPortrait;
    icon.preserveAspect = true;

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
    detailsText.text = npcText;

    // Assign slider value
    affectionGauge.value = Affection_System.Instance.GetAffectionLevel(npcData);
  }
  #endregion

  #region Show/Hide UI Helper Methods
  /// <summary>
  /// Disable afffection gauge and detail text. Enable recipe image. Fill right page's buttons with correct dishes
  /// </summary>
  public void ShowDishTab()
  {
    detailsText.gameObject.SetActive(false);
    affectionGauge.gameObject.SetActive(false);
    recipeImage.gameObject.SetActive(true);

    currentTab = Tabs.Dish;
    FillRightPage();
  }

  /// <summary>
  /// Disable affection gauge and recipe image. Enable details text. Fill right page's buttons with correct ingredients
  /// </summary>
  public void ShowIngredientTab()
  {
    detailsText.gameObject.SetActive(true);
    affectionGauge.gameObject.SetActive(false);
    recipeImage.gameObject.SetActive(false);

    currentTab = Tabs.Ingredient;
    FillRightPage();
  }

  /// <summary>
  /// Disable recipe image. Enable details text and affection gauge. Fill right page's buttons with correct npcs.
  /// </summary>
  public void ShowNPCTab()
  {
    detailsText.gameObject.SetActive(true);
    affectionGauge.gameObject.SetActive(true);
    recipeImage.gameObject.SetActive(false);

    currentTab = Tabs.NPC;
    FillRightPage();
  }

  /// <summary>
  /// Fills right page's buttons with correct images and names for the current tab's current page.
  /// Current tab's current page should be within the correct bounds, so no out of bounds exception should occur here.
  /// </summary>
  private void FillRightPage()
  {
    int currentPage = tabCurrentPage[currentTab];

    if (currentTab == Tabs.Dish)
      PopulateDishes(currentPage);
    else if (currentTab == Tabs.Ingredient)
      PopulateIngredients(currentPage);
    else if (currentTab == Tabs.NPC)
      PopulateNPCs(currentPage);
  }

  /// <summary>
  /// Flip page to next page (to the right)
  /// </summary>
  public void FlipToNext()
  {
    if (tabCurrentPage[currentTab] < tabMaxPages[currentTab])
      tabCurrentPage[currentTab] += 1;
    else
    {
      Debug.Log("[Journal_Menu]: There is no next page to current tab's current page.");
    }
  }

  /// <summary>
  /// Flip page to previous page (to the left)
  /// </summary>
  public void FlipToPrevious()
  {
    if (tabCurrentPage[currentTab] > 0)
      tabCurrentPage[currentTab] -= 1;
    else
    {
      Debug.Log("[Journal_Menu]: There is no previous page to current tab's current page.");
    }
  }
  #endregion

  [System.Serializable]
  public class JournalSlot
  {
    public Button button;
    public TextMeshProUGUI nameText;
    public Image iconImage;
    public Sprite lockedIcon;

    private object currentItem;

    public void SetItem(object item, bool unlocked)
    {
      currentItem = item;

      // Set name and icon based on type
      if (item is Dish_Data dish)
      {
        nameText.text = unlocked ? dish.Name : "?";
        iconImage.sprite = unlocked ? dish.Image : lockedIcon;
      }
      else if (item is Ingredient_Data ingredient)
      {
        nameText.text = unlocked ? ingredient.Name : "?";
        iconImage.sprite = unlocked ? ingredient.Image : lockedIcon;
      }
      else if (item is CustomerData npc)
      {
        nameText.text = unlocked ? npc.customerName : "?";
        iconImage.sprite = unlocked ? npc.defaultPortrait : lockedIcon;
      }

      // Set the button click
      button.onClick.RemoveAllListeners();
      if (unlocked)
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
      if (currentItem is Dish_Data dish)
        Instance.DisplayDishDetails(dish);
      else if (currentItem is Ingredient_Data ingredient)
        Instance.DisplayIngredientDetails(ingredient);
      else if (currentItem is CustomerData npc)
        Instance.DisplayNPCDetails(npc);
    }
  }
}
