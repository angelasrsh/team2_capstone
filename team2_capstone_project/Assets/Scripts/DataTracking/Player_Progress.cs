using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[CreateAssetMenu(menuName = "Saves/PlayerProgress")]
public class Player_Progress : ScriptableObject
{
  public static Player_Progress Instance;

  [SerializeField] private string playerName = "Chef";
  [SerializeField] private bool introPlayed = false;
  [HideInInspector] public bool isInGameplayTutorial = false;
  [HideInInspector] public bool InGameplayTutorial => isInGameplayTutorial;

  [Header("Default Unlocks")]
  [SerializeField] private Dish_Data.Dishes[] defaultDishes;
  [SerializeField] private CustomerData.NPCs[] defaultNPCs;
  [SerializeField] private IngredientType[] defaultIngredients;

  // HashSet ensures no duplicates and fast lookups
  [SerializeField] private List<Dish_Data.Dishes> unlockedDishesList = new List<Dish_Data.Dishes>();
  private HashSet<Dish_Data.Dishes> unlockedDishes = new HashSet<Dish_Data.Dishes>();
  [SerializeField] private List<CustomerData.NPCs> unlockedNPCsList = new List<CustomerData.NPCs>();
  private HashSet<CustomerData.NPCs> unlockedNPCs = new HashSet<CustomerData.NPCs>();
  [SerializeField] private List<IngredientType> unlockedIngredientsList = new List<IngredientType>();
  private HashSet<IngredientType> unlockedIngredients = new HashSet<IngredientType>();

  [Header("Money Variables")]
  [SerializeField] private float startingMoney;
  [HideInInspector] public float money;
  public static event System.Action<float> OnMoneyChanged;  

  [Header("Daily Recipe Spawner Tracking")]
  [SerializeField] private int lastRecipeSpawnedDay = -1;   // Monday=0, Tuesday=1, etc.
  [SerializeField] private Dish_Data.Dishes? activeDailyRecipe = null;
  [SerializeField] private bool hasCollectedRecipeToday = false;
  [SerializeField] private bool tutorialCustomerDialogDone = false;
  public bool TutorialCustomerDialogDone => tutorialCustomerDialogDone;
  
  public static event Action OnRecipesUpdated; // Event to notify when a dish is unlocked
  public static event Action OnNPCUnlocked; // Event to notify when an npc is unlocked
  public event System.Action OnIngredientUnlocked; // Event to notify when an ingredient is unlocked (not currently being used)
  [SerializeField] private HashSet<CustomerData.NPCs> introducedNPCs = new HashSet<CustomerData.NPCs>();
  public static event Action OnProgressLoaded;

   public void OnAfterDeserialize()
    {
        unlockedDishes = new HashSet<Dish_Data.Dishes>(unlockedDishesList);
    }

    public void OnBeforeSerialize()
    {
        unlockedDishesList = new List<Dish_Data.Dishes>(unlockedDishes);
    }
  
  /// <summary>
  /// Data to unlock at game start
  /// </summary>
  private void OnEnable()
  {
      Instance = this;

      // ONLY initialize defaults if no save is being restored
      if (!Save_Manager.HasLoadedGameData)
      {
          InitializeDefaults();
          money = startingMoney;
      }
  }

  private void InitializeDefaults()
  {
    foreach (var d in defaultDishes) UnlockDish(d);
    foreach (var n in defaultNPCs) UnlockNPC(n);
    foreach (var i in defaultIngredients) UnlockIngredient(i);
  }


  #region Getters and Setters for Save Data
  public PlayerProgressData GetSaveData()
  {
      return new PlayerProgressData
      {
        playerName = playerName,
        introPlayedData = introPlayed,
        isInGameplayTutorial = isInGameplayTutorial,
        money = money,
        unlockedDishes = new List<Dish_Data.Dishes>(unlockedDishes),
        unlockedNPCs = new List<CustomerData.NPCs>(unlockedNPCs),
        unlockedIngredients = new List<IngredientType>(unlockedIngredients),
        lastRecipeSpawnedDay = lastRecipeSpawnedDay,
        activeDailyRecipe = activeDailyRecipe,
        hasCollectedRecipeToday = hasCollectedRecipeToday,
        introducedNPCs = new List<CustomerData.NPCs>(introducedNPCs),
        tutorialCustomerDialogDone = this.tutorialCustomerDialogDone,
      };
  }

    public void LoadFromSaveData(PlayerProgressData data)
    {
        if (data == null)
        {
            Debug.LogWarning("PlayerProgressData is null, initializing new save data and tutorial progress.");

            InitializeDefaults();
            lastRecipeSpawnedDay = -1;
            hasCollectedRecipeToday = false;
            activeDailyRecipe = null;

            return;
        }

        if (!string.IsNullOrWhiteSpace(data.playerName))
            playerName = data.playerName;
        introPlayed = data.introPlayedData;
        isInGameplayTutorial = data.isInGameplayTutorial;

        if (data.unlockedDishes == null || data.unlockedDishes.Count == 0)
            InitializeDefaults();
        else
            unlockedDishes = new HashSet<Dish_Data.Dishes>(data.unlockedDishes);

        if (data.unlockedNPCs == null || data.unlockedNPCs.Count == 0)
            InitializeDefaults();
        else
            unlockedNPCs = new HashSet<CustomerData.NPCs>(data.unlockedNPCs);

        if (data.unlockedIngredients == null || data.unlockedIngredients.Count == 0)
            InitializeDefaults();
        else
            unlockedIngredients = new HashSet<IngredientType>(data.unlockedIngredients);
            
        money = data.money;
        lastRecipeSpawnedDay = data.lastRecipeSpawnedDay;
        activeDailyRecipe = data.activeDailyRecipe;
        hasCollectedRecipeToday = data.hasCollectedRecipeToday;
        introducedNPCs = new HashSet<CustomerData.NPCs>(data.introducedNPCs);
        tutorialIngreidientsGiven = false;
        tutorialCustomerDialogDone = data.tutorialCustomerDialogDone;

        OnMoneyChanged?.Invoke(money);
        OnProgressLoaded?.Invoke();
    }
    #endregion


    #region Player Name
    public void SetPlayerName(string name)
    {
        // Trim leading/trailing spaces
        name = name?.Trim();

        // If empty or null, fall back to default
        if (string.IsNullOrWhiteSpace(name))
        {
            playerName = "Chef";
        }
        else
        {
            // Limit to 16 characters max
            if (name.Length > 16)
            {
                Debug.LogWarning($"Player name '{name}' is too long — trimming to 16 characters.");
                name = name.Substring(0, 16);
            }

            playerName = name;
        }

        // Auto-save when name changes
        Save_Manager.instance?.SaveGameData();
    }

    public string GetPlayerName() => playerName;
    #endregion


    #region Intro Sequence
    public bool GetIntroPlayed() => introPlayed;
    public void SetIntroPlayed(bool played)
    {
        introPlayed = played;
        Save_Manager.instance?.SaveGameData();
    }
    #endregion


    #region Tutorial Mode
    public void SetGameplayTutorial(bool value)
    {
        isInGameplayTutorial = value;
        Save_Manager.instance?.SaveGameData();
    }

    private bool tutorialIngreidientsGiven = false;
    public void CheckAndGiveTutorialIngredients()
    {
        if (!tutorialIngreidientsGiven)
        {
            GiveTutorialStartingIngredients();
            tutorialIngreidientsGiven = true;
        }
    }

    private void GiveTutorialStartingIngredients()
    {
        Ingredient_Inventory inv = Ingredient_Inventory.Instance;
        inv.AddResources(IngredientType.Bone, 3);
        inv.AddResources(IngredientType.Uncut_Fogshroom, 5);
        inv.AddResources(IngredientType.Uncut_Fermented_Eye, 5);

        Debug.Log("Added tutorial starting ingredients to inventory.");
    }
    #endregion


    #region Money
    /// <summary>
    /// Add amount to player's money (float-safe)
    /// </summary>
    public void AddMoney(float amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    /// <summary>
    /// Subtract amount from player's money (float-safe)
    /// </summary>
    public void SubtractMoney(float amount)
    {
        money -= amount;
        OnMoneyChanged?.Invoke(money);
    }

    /// <summary>
    /// Directly sets player's money to specific value
    /// </summary>
    public void SetMoney(float amount)
    {
        money = amount;
        OnMoneyChanged?.Invoke(money);
    }

    /// <summary>
    /// Returns player's current money
    /// </summary>
    public float GetMoneyAmount() => money;
    #endregion


    #region Dishes
    /// <summary>
    /// Unlock a dish by enum (old version)
    /// </summary>
    // public void UnlockDish(Dish_Data.Dishes dish)
    // {
    //   // HashSet.Add returns true if item was actually added
    //   if (unlockedDishes.Add(dish))
    //   {
    //     // Fire event only if a new dish was added
    //     OnDishUnlocked?.Invoke();
    //     Save_Manager.instance?.SaveGameData();  // auto-save on new dish unlock
    //   }
    // }

    /// <summary>
    /// Unlock a dish by enum (new version)
    /// </summary>
    public void UnlockDish(Dish_Data.Dishes newDish)
    {
        if (!unlockedDishes.Contains(newDish))
        {
            unlockedDishes.Add(newDish);
            OnRecipesUpdated?.Invoke();
            Save_Manager.instance?.SaveGameData();  // auto-save on new dish unlock
        }
    }

    /// <summary>
    /// Check if a dish is unlocked
    /// </summary>
    public bool IsDishUnlocked(Dish_Data.Dishes dish) => unlockedDishes.Contains(dish);

    /// <summary>
    /// Get all unlocked dishes as a list
    /// </summary>
    public List<Dish_Data.Dishes> GetUnlockedDishes() => new List<Dish_Data.Dishes>(unlockedDishes);

    public bool HasCollectedRecipeToday() => hasCollectedRecipeToday;

    public void MarkRecipeCollectedToday()
    {
        hasCollectedRecipeToday = true;
        Save_Manager.instance?.SaveGameData();
    }

    public void ResetDailyRecipeFlags(bool fullReset = false)
    {
        activeDailyRecipe = null;
        hasCollectedRecipeToday = false;

        if (fullReset)
            lastRecipeSpawnedDay = -1; // resets day tracking completely

        Save_Manager.instance?.SaveGameData();
    }

    public void ResetRecipeUnlocks()
    {
        unlockedDishes.Clear();
        InitializeDefaults();
    }
    #endregion


    #region Ingredients
    /// <summary>
    /// Unlock an ingredient by Ingredient_Data
    /// </summary>
    public void UnlockIngredient(IngredientType ingr)
    {
        if (unlockedIngredients.Add(ingr))
        {
        // Fire event only if a new ingredient was added
        OnIngredientUnlocked?.Invoke();
        }
    }

    /// <summary>
    /// Check if an ingredient is unlocked
    /// </summary>
    public bool IsIngredientUnlocked(IngredientType ingr) => unlockedIngredients.Contains(ingr);

    /// <summary>
    /// Get all unlocked ingredients as a list
    /// </summary>
    public List<IngredientType> GetUnlockedIngredients() => new List<IngredientType>(unlockedIngredients);
    #endregion


    #region NPCs
    /// <summary>
    /// Unlock an NPC by enum
    /// </summary>
    public void UnlockNPC(CustomerData.NPCs npc)
    {
        if (unlockedNPCs.Add(npc))
        {
        // Fire event only if a new npc was added
        OnNPCUnlocked?.Invoke();
        }
    }

    /// <summary>
    /// Check if an NPC is unlocked
    /// </summary>
    public bool IsNPCUnlocked(CustomerData.NPCs npc) => unlockedNPCs.Contains(npc);

    /// <summary>
    /// Get all unlocked NPCs as a list
    /// </summary>
    public List<CustomerData.NPCs> GetUnlockedNPCs() => new List<CustomerData.NPCs>(unlockedNPCs);

    public bool HasMetNPC(CustomerData.NPCs npc)
    {
        return introducedNPCs.Contains(npc);
    }

    public void MarkNPCIntroduced(CustomerData.NPCs npc)
    {
        if (introducedNPCs.Add(npc))
        {
            Save_Manager.instance?.SaveGameData();
        }
    }

    public void MarkTutorialDialogDone()
    {
        tutorialCustomerDialogDone = true;
        Save_Manager.instance?.SaveGameData();
    }
    #endregion


    #region Daily Recipe Spawner Tracking
    public int GetLastRecipeSpawnedDay() => lastRecipeSpawnedDay;
    public Dish_Data.Dishes? GetActiveDailyRecipe() => activeDailyRecipe;

    public void SetDailyRecipe(Dish_Data.Dishes dish)
    {
        activeDailyRecipe = dish;
        Save_Manager.instance?.SaveGameData();
    }

    public void MarkDailyRecipeSpawned(int dayIndex)
    {
        lastRecipeSpawnedDay = dayIndex;
        Save_Manager.instance?.SaveGameData();
    }

    public void ClearDailyRecipe()
    {
        activeDailyRecipe = null;
        Save_Manager.instance?.SaveGameData();
    }
    #endregion
}

#region PlayerProgressData
[System.Serializable]
public class PlayerProgressData
{
    public string playerName = "";
    public bool introPlayedData = false;
    public bool isInGameplayTutorial = false;
    public List<Dish_Data.Dishes> unlockedDishes = new();
    public List<CustomerData.NPCs> unlockedNPCs = new();
    public List<IngredientType> unlockedIngredients = new();
    public float money;
    public int lastRecipeSpawnedDay = -1;
    public Dish_Data.Dishes? activeDailyRecipe = null;
    public bool hasCollectedRecipeToday = false; 
    public List<CustomerData.NPCs> introducedNPCs = new();
    public bool tutorialIngreidientsGiven = false;
    public bool tutorialCustomerDialogDone;
}
#endregion
