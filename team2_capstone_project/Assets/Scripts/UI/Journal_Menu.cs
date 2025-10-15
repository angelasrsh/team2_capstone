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
  private bool haveTabsInitialized = false;
  private Player_Progress playerProgress = Player_Progress.Instance;
  private InputAction openJournalAction, closeJournalAction, flipLeftAction, flipRightAction, clickAction;
  private int objectsPerPage = 6; // right page only fits 6 objects per page with current cell size (if changed, change slots list in inspector too)
  private Choose_Menu_Items dailyMenu;

  [Header("Databases")]
  private Dish_Database dishDatabase;
  private Ingredient_Database ingredientDatabase;
  private NPC_Database npcDatabase;

  [Header("Data Lists")]
  private List<Dish_Data> allDishes;
  private List<Ingredient_Data> allIngredients;
  private List<CustomerData> allNPCs;

  [Header("Journal GameObject References")]
  private GameObject darkOverlay; // journal's dark overlay child
  private GameObject journalContents; // journal's background child
  private GameObject tabs; // journal's tab child

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

  [Header("UI References and Variables")]
  [SerializeField] public Sprite LockedIcon; // generic locked icon for locked items (just a question mark)
  private Tabs currentTab = Tabs.None;

  // Dictionary allows players to open to last page they had open in each tab
  private Dictionary<Tabs, int> tabCurrentPage = new Dictionary<Tabs, int>()
  {
    { Tabs.Dish, 1 },
    { Tabs.Ingredient, 1 },
    { Tabs.NPC, 1 }
  };
  private Dictionary<Tabs, int> tabMaxPages;
  public enum Tabs
  {
    None,
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
    if (scene.name == "Main_Menu")
      gameObject.SetActive(false);
    else
      gameObject.SetActive(true);
  }

  private void Start()
  {
    dishDatabase = Game_Manager.Instance.dishDatabase;
    ingredientDatabase = Game_Manager.Instance.ingredientDatabase;
    npcDatabase = Game_Manager.Instance.npcDatabase;

    allDishes = Game_Manager.Instance.dishDatabase.dishes;
    allIngredients = Game_Manager.Instance.ingredientDatabase.rawForagables;
    // If wanting to use all ingredients list instead of showing only the raw foragable ones:
    // allIngredients = Game_Manager.Instance.ingredientDatabase.allIngredients;
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

    if (detailsText == null)
      Debug.LogError("[Journal_Menu]: detailsText not assigned in inspector!");

    if (recipeImage == null)
      Debug.LogError("[Journal_Menu]: recipeImage not assigned in inspector!");

    if (affectionGauge == null)
      Debug.LogError("[Journal_Menu]: affectionGauge not assigned in inspector!");

    if (Choose_Menu_Items.instance == null)
      Debug.LogWarning("Choose_Menu_Items instance is NULL in this scene!");
    else
    {
      Debug.Log("Choose_Menu_Items still alive. Dishes count: " + Choose_Menu_Items.instance.GetSelectedDishes().Count);
      dailyMenu = Choose_Menu_Items.instance;
    }

    Choose_Menu_Items.OnMenuSelectedNoParams += PopulateDishes; // to show stars on dishes in journal that are on daily menu

    ShowDishTab(); // default to dish tab
    leftPagePanel.SetActive(false); // hide left page details at start
    StartCoroutine(DelayedHideJournal());
  }

  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoadedRebind;
    TryBindInput();
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= OnSceneLoadedRebind;
    if (openJournalAction != null)
      openJournalAction.Disable();
    if (closeJournalAction != null)
      closeJournalAction.Disable();
    if (flipLeftAction != null)
      flipLeftAction.Disable();
    if (flipRightAction != null)
      flipRightAction.Disable();
    if (clickAction != null)
      clickAction.Disable();
  }

  private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode)
  {
    TryBindInput();
  }

  private void TryBindInput()
  {
    // Find the active PlayerInput in the scene (e.g. on Game_Manager)
    if (Game_Manager.Instance == null)
    {
      Debug.LogWarning("[Journal_Menu] No Game_Manager instance found to bind journal inputs.");
      return;
    }
    PlayerInput playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
    if (playerInput == null)
    {
      Debug.LogWarning("[Journal_Menu] No PlayerInput found to bind journal inputs.");
      return;
    }

    // Bind the "OpenJournal" action (Player Map)
    openJournalAction = playerInput.actions["OpenJournal"];
    openJournalAction.Enable();

    // Bind Journal actions (Journal Map)
    flipLeftAction = playerInput.actions["FlipLeft"];
    flipRightAction = playerInput.actions["FlipRight"];
    closeJournalAction = playerInput.actions["CloseJournal"];
    clickAction = playerInput.actions["Click"];

    flipLeftAction.performed += ctx => FlipToPrevious();
    flipRightAction.performed += ctx => FlipToNext();
    closeJournalAction.performed += ctx => ResumeGame();
    // clickAction.performed += ctx => OnClick();

    flipLeftAction.Disable();
    flipRightAction.Disable();
    closeJournalAction.Disable();
    clickAction.Disable();
  }

  private void OnDestroy()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
    Choose_Menu_Items.OnMenuSelectedNoParams -= PopulateDishes;
  }

  // Update is called once per frame
  private void Update()
  {
    if (openJournalAction.WasPerformedThisFrame())
    {
      if (isPaused)
      {
        ResumeGame();
        Game_Events_Manager.Instance.JournalToggled(false); // this is being checked every time the journal is opened/closed. Might not be ideal
      }
      else
      {
        PauseGame();
        Game_Events_Manager.Instance.JournalToggled(true);
      }
    }
  }

  private IEnumerator DelayedHideJournal()
  {
    yield return new WaitForEndOfFrame(); // wait one frame to avoid null references
    ResumeGame(false); // don't play sound on initial hide
  }

  public void PauseGame()
  {
      Debug.Log("Opening journal and pausing game...");
      Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.bookOpen, 0.75f);
      darkOverlay.SetActive(true);
      journalContents.SetActive(true);
      tabs.SetActive(true);
      isPaused = true;

      // Get current PlayerInput
      PlayerInput playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

    // Switch to the "Journal" input map
    if (playerInput != null)
    {
      playerInput.SwitchCurrentActionMap("Journal");
      flipLeftAction.Enable();
      flipRightAction.Enable();
      closeJournalAction.Enable();
      clickAction.Enable();
    }
      
    // Disable player movement  
    Player_Controller player = FindObjectOfType<Player_Controller>();
    if (player != null)
        player.DisablePlayerMovement();
  }

  public void ResumeGame(bool playSound = true)
  {
      Debug.Log("Closing journal and resuming game...");
      if (playSound)
          Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.bookClose, 0.75f);

      darkOverlay.SetActive(false);
      journalContents.SetActive(false);
      tabs.SetActive(false);
      isPaused = false;
      
       // Get current PlayerInput
      PlayerInput playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

    // Switch back to Player map
    if (playerInput != null)
    {
      flipLeftAction.Disable();
      flipRightAction.Disable();
      closeJournalAction.Disable();
      clickAction.Disable();
      playerInput.SwitchCurrentActionMap("Player");
    }

    // Enable player movement
    Player_Controller player = FindObjectOfType<Player_Controller>();
    if (player != null)
        player.EnablePlayerMovement();
  }

  #region Recipe Tab Methods
  private void PopulateDishes()
  {
    // Debug.Log("Populating dishes for current page in journal...");
    int currentPage = tabCurrentPage[currentTab];
    List<Dish_Data.Dishes> unlockedDishes = playerProgress.GetUnlockedDishes();
    int currGridCell = 0;
    int startIndex = (currentPage - 1) * objectsPerPage;
    int index = startIndex + currGridCell;

    // Populate with unlocked dishes if needed
    while (currGridCell < objectsPerPage)
    {
      JournalSlot slot = slots[currGridCell];
      if (index < unlockedDishes.Count)
      {
        Dish_Data dishData = dishDatabase.GetDish(unlockedDishes[index]);

        // Check if dish is on daily menu
        if (dailyMenu != null && dailyMenu.GetSelectedDishes().Contains(dishData.dishType))
            slot.SetItem(dishData, true, true);
        else
          slot.SetItem(dishData, true, false);
      }
      else if (index < allDishes.Count)
      {
        // Show locked dishes only if there are still more dishes in the database
        slot.SetItem(null, false, false);
      }
      else
      {
        // Don't show empty slots if no more NPCs to show
        slot.ClearItem();
      }

      index++;
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

  #region Ingredient Tab Methods
  private void PopulateIngredients()
  {
    // Debug.Log("Populating ingredients for current page in journal...");
    int currentPage = tabCurrentPage[currentTab];
    List<IngredientType> unlockedIngredients = playerProgress.GetUnlockedIngredients();
    int currGridCell = 0;
    int startIndex = (currentPage - 1) * objectsPerPage;
    int index = startIndex + currGridCell;

    // Using all ingredients list
    // while (currGridCell < objectsPerPage)
    // {
    //   JournalSlot slot = slots[currGridCell];
    //   if (index < unlockedIngredients.Count)
    //   {
    //     Ingredient_Data ingredientData = ingredientDatabase.GetIngredient(unlockedIngredients[index]);
    //     slot.SetItem(ingredientData, true, false);
    //   }
    //   else if (index < allIngredients.Count)
    //   {
    //     // Show locked ingredients only if there are still more ingredients in the database
    //     slot.SetItem(null, false, false);
    //   }
    //   else
    //   {
    //     // Don't show empty slots if no more ingredients to show
    //     slot.ClearItem();
    //   }

    //   index++;
    //   currGridCell++;
    // }

    // using just foragables
    while (currGridCell < objectsPerPage)
    {
      JournalSlot slot = slots[currGridCell];
      if (index < allIngredients.Count)
      {
        Ingredient_Data ingredientData = allIngredients[index];
        bool isUnlocked = unlockedIngredients.Contains(ingredientData.ingredientType);
        if(isUnlocked)
          slot.SetItem(ingredientData, true, false);
        else // Show locked ingredients only if there are still more ingredients in the database
          slot.SetItem(null, false, false);
      }
      else
      {
        // Don't show empty slots if no more NPCs to show
        slot.ClearItem();
      }

      index++;
      currGridCell++;
    }
  }

  private void DisplayIngredientDetails(Ingredient_Data ingredient)
  {
    leftPagePanel.SetActive(true);

    // Ingredient Info
    nameText.text = ingredient.Name;
    icon.sprite = ingredient.Image;
    icon.preserveAspect = true;

    // Collect ingredient lines
    Ingredient_Requirement[] makesIngredientArray = ingredient.makesIngredient.ToArray();
    Dish_Data[] usedInDishesArray = ingredient.usedInDishes.ToArray();

    string ingredientText = "Description: \n" + ingredient.description + "\n\n";

    ingredientText += "Rarity: " + ingredient.rarityWeight + "\n";

    ingredientText += "Used to make ingredients: " +
        (makesIngredientArray.Length > 0
            ? string.Join(", ", makesIngredientArray.Select(d => d.ingredient.Name))
            : "None") + "\n";

    ingredientText += "Used to make dishes: " +
        (usedInDishesArray.Length > 0
            ? string.Join(", ", usedInDishesArray.Select(d => d.Name))
            : "None");

    // Assign once to text component
    detailsText.text = ingredientText;

    // Broadcast event for tutorial
    // Game_Events_Manager.Instance.ForageDetailsClick();
  }
  #endregion

  #region NPC Tab Methods
  private void PopulateNPCs()
  {
    // Debug.Log("Populating npcs for current page in journal...");
    int currentPage = tabCurrentPage[currentTab];
    List<CustomerData.NPCs> unlockedNPCs = playerProgress.GetUnlockedNPCs();
    int currGridCell = 0;
    int startIndex = (currentPage - 1) * objectsPerPage;
    int index = startIndex + currGridCell;

    // Populate with unlocked dishes if needed
    while (currGridCell < objectsPerPage)
    {
      JournalSlot slot = slots[currGridCell];
      if (index < unlockedNPCs.Count)
      {
        CustomerData NPC = npcDatabase.GetNPCData(unlockedNPCs[index]);
        slot.SetItem(NPC, true, false);
      }
      else if (index < allNPCs.Count)
      {
        // Show locked NPCs only if there are still more NPCs in the database
        slot.SetItem(null, false, false);
      }
      else
      {
        // Don't show empty slots if no more NPCs to show
        slot.ClearItem();
      }

      index++;
      currGridCell++;
    }
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

    npcText += "Dateable: " + (npcData.datable ? "YES" : "no") + "\n";

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

    // Assign slider value if datable
    if (npcData.datable)
    {
      affectionGauge.value = Affection_System.Instance.GetAffectionLevel(npcData);
      affectionGauge.gameObject.SetActive(true);
    }
    else
      affectionGauge.gameObject.SetActive(false);
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

      bool isSwitchingTabs = currentTab != Tabs.Dish;
      if (isSwitchingTabs)
      {
          leftPagePanel.SetActive(false);
          currentTab = Tabs.Dish;
          FillRightPage();

          // Only play sound if tabs have already been initialized once
          if (haveTabsInitialized)
              PlayJournalTabSelectSound();
      }

      // After first tab setup, mark as initialized
      haveTabsInitialized = true;
  }

  /// <summary>
  /// Disable affection gauge and recipe image. Enable details text. Fill right page's buttons with correct ingredients
  /// </summary>
  public void ShowIngredientTab()
  {
      detailsText.gameObject.SetActive(true);
      affectionGauge.gameObject.SetActive(false);
      recipeImage.gameObject.SetActive(false);

      bool isSwitchingTabs = currentTab != Tabs.Ingredient;
      if (isSwitchingTabs)
      {
          leftPagePanel.SetActive(false);
          currentTab = Tabs.Ingredient;
          FillRightPage();

          if (haveTabsInitialized)
              PlayJournalTabSelectSound();
      }

      haveTabsInitialized = true;
  }

  /// <summary>
  /// Disable recipe image. Enable details text. Fill right page's buttons with correct npcs. Affection gauge should
  /// only show up for the npcs that are datable, so don't enable it here (enabled in DisplayNPCDetails if datable)
  /// </summary>
  public void ShowNPCTab()
  {
      detailsText.gameObject.SetActive(true);
      recipeImage.gameObject.SetActive(false);

      bool isSwitchingTabs = currentTab != Tabs.NPC;
      if (isSwitchingTabs)
      {
          leftPagePanel.SetActive(false);
          currentTab = Tabs.NPC;
          FillRightPage();

          if (haveTabsInitialized)
              PlayJournalTabSelectSound();
      }

      haveTabsInitialized = true;
  }

  /// <summary>
  /// Fills right page's buttons with correct images and names for the current tab's current page.
  /// Current tab's current page should be within the correct bounds, so no out of bounds exception should occur here.
  /// </summary>
  private void FillRightPage()
  {
    if (currentTab == Tabs.Dish)
      PopulateDishes();
    else if (currentTab == Tabs.Ingredient)
      PopulateIngredients();
    else if (currentTab == Tabs.NPC)
      PopulateNPCs();
  }

  /// <summary>
  /// Flip page to next page (to the right)
  /// </summary>
  public void FlipToNext()
  {
    Debug.Log("Flipping to next page...");
    Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.pageFlip, 0.8f);

    if (tabCurrentPage[currentTab] < tabMaxPages[currentTab])
    {
      tabCurrentPage[currentTab] += 1;
      FillRightPage();
      leftPagePanel.SetActive(false); // hide left page details when flipping to next page, since no item selected yet
    }
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
    Debug.Log("Flipping to previous page...");
    Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.pageFlip, 0.8f);

    if (tabCurrentPage[currentTab] > 1)
    {
      tabCurrentPage[currentTab] -= 1;
      FillRightPage();
      leftPagePanel.SetActive(false); // hide left page details when flipping to previous page, since no item selected yet
    }
    else
    {
      Debug.Log("[Journal_Menu]: There is no previous page to current tab's current page.");
    }
  }
  #endregion

  [System.Serializable]
  public class JournalSlot
  {
    public GameObject background; // the button's background object, used to turn off when no item
    public Button button;
    public TextMeshProUGUI nameText;
    public Image iconImage;
    private object currentItem;
    public Image starImage;

    public void SetItem(object item, bool unlocked, bool onDailyMenu)
    {
      background.SetActive(true);
      currentItem = item;

      // Set name and icon based on type
      if (!unlocked)
      {
        nameText.text = "Unknown"; // if we want to show ? for the name, then replace with "?"
        iconImage.sprite = Instance.LockedIcon;
      }
      else if (item is Dish_Data dish)
      {
        nameText.text = dish.Name;
        iconImage.sprite = dish.Image;
      }
      else if (item is Ingredient_Data ingredient)
      {
        nameText.text = ingredient.Name;
        iconImage.sprite = ingredient.Image;
      }
      else if (item is CustomerData npc)
      {
        nameText.text = npc.customerName;
        iconImage.sprite = npc.defaultPortrait;
      }
      iconImage.preserveAspect = true;

      if (onDailyMenu)
        starImage.gameObject.SetActive(true);
      else
        starImage.gameObject.SetActive(false);
      starImage.preserveAspect = true;

      // Set the button click
      button.onClick.RemoveAllListeners();
      if (unlocked)
        button.onClick.AddListener(OnClick);
    }

    public void ClearItem()
    {
      currentItem = null;
      background.SetActive(false);
      button.onClick.RemoveAllListeners();
    }

    private void OnClick()
    {
      if (currentItem is Dish_Data dish)
        Instance.DisplayDishDetails(dish);
      else if (currentItem is Ingredient_Data ingredient)
        Instance.DisplayIngredientDetails(ingredient);
      else if (currentItem is CustomerData npc)
        Instance.DisplayNPCDetails(npc);

      Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.clickSFX, 0.8f);
    }
  }
  
  private void PlayJournalTabSelectSound()
  {
    Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.journalTabSelect, 0.4f);
  }
}
