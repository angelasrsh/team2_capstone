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

  [Header("Default Unlocks")]
  [SerializeField] private Dish_Data.Dishes[] defaultDishes;
  [SerializeField] private CustomerData.NPCs[] defaultNPCs;
  [SerializeField] private IngredientType[] defaultIngredients;

  // HashSet ensures no duplicates and fast lookups
  [SerializeField] private HashSet<Dish_Data.Dishes> unlockedDishes = new HashSet<Dish_Data.Dishes>();
  [SerializeField] private HashSet<CustomerData.NPCs> unlockedNPCs = new HashSet<CustomerData.NPCs>();
  [SerializeField] private HashSet<IngredientType> unlockedIngredients = new HashSet<IngredientType>();

  // Money vars
  [SerializeField] private float startingMoney;
  [HideInInspector] public float money;
  public static event System.Action<float> OnMoneyChanged;  

  // Daily Recipe Spawner tracking
  [SerializeField] private int lastRecipeSpawnedDay = -1;   // Monday=0, Tuesday=1, etc.
  [SerializeField] private Dish_Data.Dishes? activeDailyRecipe = null;
  public static event Action OnRecipesUpdated;
  [SerializeField] private bool hasCollectedRecipeToday = false;
  
  public event System.Action OnDishUnlocked; // Event to notify when a dish is unlocked (not currently being used)
  public event System.Action OnNPCUnlocked; // Event to notify when an npc is unlocked (not currently being used)
  public event System.Action OnIngredientUnlocked; // Event to notify when an ingredient is unlocked (not currently being used)


  /// <summary>
  /// Data to unlock at game start
  /// </summary>
  private void OnEnable()
  {
    Instance = this;
    InitializeDefaults();
    money = startingMoney;
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
          money = money,
          unlockedDishes = new List<Dish_Data.Dishes>(unlockedDishes),
          unlockedNPCs = new List<CustomerData.NPCs>(unlockedNPCs),
          unlockedIngredients = new List<IngredientType>(unlockedIngredients),
          lastRecipeSpawnedDay = lastRecipeSpawnedDay,
          activeDailyRecipe = activeDailyRecipe,
          hasCollectedRecipeToday = hasCollectedRecipeToday
      };
  }

  public void LoadFromSaveData(PlayerProgressData data)
  {
      if (data == null)
      {
          Debug.LogWarning("PlayerProgressData is null, loading defaults.");
          InitializeDefaults();
          lastRecipeSpawnedDay = -1;
          hasCollectedRecipeToday = false;
          activeDailyRecipe = null;
          return;
      }

      playerName = string.IsNullOrWhiteSpace(data.playerName) ? "Chef" : data.playerName;
      unlockedDishes = new HashSet<Dish_Data.Dishes>(data.unlockedDishes);
      unlockedNPCs = new HashSet<CustomerData.NPCs>(data.unlockedNPCs);
      unlockedIngredients = new HashSet<IngredientType>(data.unlockedIngredients);
      money = data.money;
      lastRecipeSpawnedDay = data.lastRecipeSpawnedDay;
      activeDailyRecipe = data.activeDailyRecipe;
      hasCollectedRecipeToday = data.hasCollectedRecipeToday;

      OnMoneyChanged?.Invoke(money);
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
    public string playerName = "Chef";
    public List<Dish_Data.Dishes> unlockedDishes = new();
    public List<CustomerData.NPCs> unlockedNPCs = new();
    public List<IngredientType> unlockedIngredients = new();
    public float money;
    public int lastRecipeSpawnedDay = -1;
    public Dish_Data.Dishes? activeDailyRecipe = null;
    public bool hasCollectedRecipeToday = false; 
}
#endregion
