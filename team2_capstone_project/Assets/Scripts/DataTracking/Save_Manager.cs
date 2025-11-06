using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Grimoire;

public class Save_Manager : MonoBehaviour
{
    public static Save_Manager instance;
    private static GameData currentGameData;
    public Room_Collection_Data roomCollection;
    private RoomExitOptions roomExitOptions;
    private Grimoire.Screen_Fade screenFade;

    private string saveFilePath;
    private int currentSaveSlot = 1;
    private float tempElapsedTime = 0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            RoomManager.Initialize(roomCollection);
        }
        else
            Destroy(gameObject);
    }

    private void Update() => AddElapsedTime(Time.deltaTime);

    /// <summary>
    /// Add elapsed time to current game data or temporary storage if no data exists.
    /// </summary>
    /// <param name="time"></param>
    public void AddElapsedTime(float time)
    {
        if (currentGameData != null)
            currentGameData.elapsedTime += time;
        else
            tempElapsedTime += time;
    }

    public void SetCurrentSlot(int slot)
    {
        currentSaveSlot = slot;
        Debug.Log($"Active save slot set to: {slot}");
    }


    /// <summary>
    /// Get the current game data, or load from specified slot if none is active.
    /// </summary>
    /// <param name="slot"></param>
    public static GameData GetGameData(int slot = -1)
    {
        // Default to currentSaveSlot if no slot is specified
        if (slot == -1)
        {
            slot = instance.currentSaveSlot;
        }

        string filePath = instance.GetFilePath(slot);

        if (File.Exists(filePath))
        {
            string dataToLoad = File.ReadAllText(filePath);
            return JsonUtility.FromJson<GameData>(dataToLoad);
        }
        else
            return null;  // No save data found
    }

    /// <summary>
    /// Set the current game data to the provided data.
    /// </summary>
    /// <param name="aData"></param>
    public static void SetGameData(GameData aData) => currentGameData = aData;

    #region Save + Load
    public void SaveGameData() => SaveGameData(currentSaveSlot);  // Default save slot is 1 if one is not provided
    public void SaveGameData(int slot)
    {
        try
        {
            // Ensure we have data to save
            if (currentGameData == null)
            {
                Debug.LogWarning("No active GameData found. Creating new one for autosave.");
                currentGameData = new GameData();
            }

            UpdateGameData(); // Fill with latest info

            string filePath = GetFilePath(slot);
            string dataToSave = JsonUtility.ToJson(currentGameData, true);
            File.WriteAllText(filePath, dataToSave);

            Debug.Log($"Game saved to Slot {slot}! Elapsed Time: {currentGameData.elapsedTime}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game to Slot {slot}: {e}");
        }
    }

    public void LoadGameData() => LoadGameData(currentSaveSlot);
    public void LoadGameData(int slot)
    {
        try
        {
            string filePath = GetFilePath(slot);

            if (File.Exists(filePath))
            {
                string dataToLoad = File.ReadAllText(filePath);
                currentGameData = JsonUtility.FromJson<GameData>(dataToLoad);

                // Reset temporary time
                tempElapsedTime = 0f;

                RestoreGameData();
                Debug.Log($"Game loaded from Slot {slot}!");
            }
            else
            {
                currentGameData = null;
                Debug.LogWarning($"No save file found in Slot {slot}.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game from Slot {slot}: {e}");
        }
    }
    #endregion


    /// <summary>
    /// Create a new save slot, overwriting any existing data in that slot.
    /// </summary>
    public void CreateNewSaveSlot(int slot)
    {
        currentGameData = new GameData
        {
            elapsedTime = 0f
        };

        // Reset player progress daily recipe flags
        Player_Progress.Instance?.ResetDailyRecipeFlags(fullReset: true);

        // Reset affection event tracker when starting a new save
        if (Affection_Event_Item_Tracker.instance != null)
        {
            Affection_Event_Item_Tracker.instance.ResetTracker();
            Affection_Event_Item_Tracker.instance.RecheckUnlocks();
        }

        SetCurrentSlot(slot);
        SaveGameData(slot);

        Debug.Log($"New save slot {slot} created (overwritten if existed).");
    }

    // Old version, keeping this just in case
    // public void CreateNewSaveSlot(int slot)
    // {
    //     if (currentGameData == null) // Only create if no active save data
    //     {
    //         currentGameData = new GameData
    //         {
    //             elapsedTime = tempElapsedTime
    //         };

    //         SetCurrentSlot(slot); // Update the current save slot
    //         SaveGameData(slot);   // Save the data to disk

    //         Debug.Log($"New save slot {slot} created with elapsed time: {tempElapsedTime}");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Save slot {slot} already exists or is being used.");
    //     }
    // }

    // Also an old version, kept for reference
    // private void UpdateGameData()
    // {
    //     var player = FindObjectOfType<Player_Controller>();
    //     if (player != null && player.currentRoom != null)
    //     {
    //         currentGameData.currentRoom = player.currentRoom.roomID.ToString();
    //         Debug.Log($"Updated current room: {currentGameData.currentRoom}");
    //     }

    //     currentGameData.playerProgress = Player_Progress.Instance.GetSaveData();

    //     // Save quest data
    //     if (Quest_Manager.Instance != null)
    //         currentGameData.questData = Quest_Manager.Instance.GetSaveData();

    //     // Save ingredient inventory data
    //     if (Ingredient_Inventory.Instance != null)
    //         currentGameData.ingredientInventoryData = Ingredient_Inventory.Instance.GetSaveData();

    //     // Save dish inventory data
    //     if (Dish_Tool_Inventory.Instance != null)
    //         currentGameData.dishInventoryData = Dish_Tool_Inventory.Instance.GetSaveData();

    //     currentGameData.elapsedTime += Time.deltaTime;
    // }

    private void UpdateGameData()
    {
        // --- Room tracking ---
        Room_Data activeRoom = Room_Manager.GetRoomFromActiveScene();
        if (activeRoom != null)
        {
            currentGameData.currentRoom = activeRoom.roomID.ToString();
            Debug.Log($"[Save_Manager] Current room saved as {currentGameData.currentRoom}");
        }
        else
        {
            Debug.LogWarning("[Save_Manager] Could not find active room! Defaulting to Kitchen.");
            currentGameData.currentRoom = "Kitchen"; // fallback
        }

        // --- Player progress ---
        if (Player_Progress.Instance != null)
            currentGameData.playerProgress = Player_Progress.Instance.GetSaveData();
        else
        {
            Debug.LogWarning("[Save_Manager] Player_Progress.Instance is null! Using defaults.");
            currentGameData.playerProgress = new PlayerProgressData();
        }

        // --- Ingredient inventory (extra check) ---
        if (Ingredient_Inventory.Instance != null)
            currentGameData.ingredientInventoryData = Ingredient_Inventory.Instance.GetSaveData();
        else
            Debug.LogWarning("[Save_Manager] Ingredient_Inventory.Instance is null!");

        // --- Restaurant state ---
        if (Restaurant_State.Instance != null)
        {
            Restaurant_State.Instance.SaveCustomers(); // ensure data is current
            currentGameData.restaurantStateData = new RestaurantStateData
            {
                customers = new List<Customer_State>(Restaurant_State.Instance.customers)
            };
            Debug.Log($"[Save_Manager] Saved {currentGameData.restaurantStateData.customers.Count} customers in restaurant state.");
        }
        else
        {
            Debug.LogWarning("[Save_Manager] Restaurant_State.Instance is null! Saving empty restaurant state.");
            currentGameData.restaurantStateData = new RestaurantStateData();
        }

        // --- Daily menu ---
        if (Choose_Menu_Items.instance != null)
        {
            currentGameData.dailyMenuData = new DailyMenuData
            {
                dailyPool = Choose_Menu_Items.instance.GetDailyPool(),
                dishesSelected = Choose_Menu_Items.instance.GetSelectedDishes(),
                customersPlanned = Day_Plan_Manager.instance != null ? Day_Plan_Manager.instance.customersPlannedForEvening : 0
            };
            Debug.Log($"[Save_Manager] Saved daily menu with {currentGameData.dailyMenuData.dishesSelected.Count} selected dishes.");
        }
        else
        {
            currentGameData.dailyMenuData = new DailyMenuData();
            Debug.LogWarning("[Save_Manager] Choose_Menu_Items.instance was null when saving daily menu!");
        }

        // --- Current day ---
        if (Day_Turnover_Manager.Instance != null)
        {
            currentGameData.currentDay = Day_Turnover_Manager.Instance.CurrentDay;
            Debug.Log($"[Save_Manager] Saved current day as {currentGameData.currentDay}");
        }
        else
            Debug.LogWarning("[Save_Manager] Day_Turnover_Manager.Instance was null during save — keeping previous day value.");

        // --- Affection reward tracker ---
        if (Affection_Event_Item_Tracker.instance != null)
        {
            currentGameData.affectionEventItems = Affection_Event_Item_Tracker.instance.GetSaveData();
            Debug.Log($"[Save_Manager] Saved {currentGameData.affectionEventItems.Count} affection event items.");
        }

        // --- Chest ---
        if (Chest.Instance != null)
        {
            currentGameData.chestData = Chest.Instance.GetChestSaveData();
            // Debug.Log($"[Save_Manager] Saved {currentGameData.chestData.itemsInChest.Count} items in chest.");
        }
        else
            Debug.LogWarning("[Save_Manager] Chest.Instance was null during save — chest data not saved.");
          
        // --- Cutscenes ---
        if (Cutscene_Manager.Instance != null)
            Cutscene_Manager.Instance.SaveToGameData(currentGameData);

        // --- Elapsed time ---
        currentGameData.elapsedTime += Time.deltaTime;
    }

    private void RestoreGameData()
    {
        // Restore player progress
        if (currentGameData.playerProgress != null)
            Player_Progress.Instance.LoadFromSaveData(currentGameData.playerProgress);
        else
            Debug.LogWarning("No PlayerProgress data found in save file.");

        // Restore quests
        if (currentGameData.questData != null)
            Quest_Manager.Instance.LoadFromSaveData(currentGameData.questData);
        else
            Debug.LogWarning("No Quest data found in save file.");

        // Restore ingredient inventory
        if (currentGameData.ingredientInventoryData != null)
            Ingredient_Inventory.Instance.LoadFromSaveData(currentGameData.ingredientInventoryData);
        else
            Debug.LogWarning("No Ingredient Inventory data found in save file.");

        // Restore dish inventory
        if (currentGameData.dishInventoryData != null)
            Dish_Tool_Inventory.Instance.LoadFromSaveData(currentGameData.dishInventoryData);
        else
            Debug.LogWarning("No Dish Inventory data found in save file.");

        // Restore restaurant state
        if (currentGameData.restaurantStateData != null && currentGameData.restaurantStateData.customers != null)
        {
            if (Restaurant_State.Instance != null)
            {
                Restaurant_State.Instance.customers = new List<Customer_State>(currentGameData.restaurantStateData.customers);
                Debug.Log($"[Save_Manager] Restored {Restaurant_State.Instance.customers.Count} customers to restaurant state.");
            }
            else
                Debug.LogWarning("[Save_Manager] Restaurant_State.Instance not found when restoring restaurant data!");
        }
        else
            Debug.Log("[Save_Manager] No restaurant state found in save file.");

        // Restore daily menu
        if (currentGameData.dailyMenuData != null && Choose_Menu_Items.instance != null)
        {
            Choose_Menu_Items.instance.LoadFromSaveData(currentGameData.dailyMenuData);

            if (Day_Plan_Manager.instance != null)
                Day_Plan_Manager.instance.SetPlan(
                    currentGameData.dailyMenuData.dishesSelected,
                    currentGameData.dailyMenuData.customersPlanned
                );

            Debug.Log($"[Save_Manager] Restored daily menu with {currentGameData.dailyMenuData.dishesSelected.Count} selected dishes.");
        }
        else
            Debug.LogWarning("[Save_Manager] No daily menu data found or Choose_Menu_Items not ready yet.");

        // Restore current day
        if (Day_Turnover_Manager.Instance != null)
            Day_Turnover_Manager.Instance.SetCurrentDay(currentGameData.currentDay);

        // Restore affection rewards
        if (currentGameData.affectionEventItems != null && Affection_Event_Item_Tracker.instance != null)
        {
            Affection_Event_Item_Tracker.instance.LoadFromSaveData(currentGameData.affectionEventItems);
            Affection_Event_Item_Tracker.instance.RecheckUnlocks();
            Debug.Log($"[Save_Manager] Restored {currentGameData.affectionEventItems.Count} affection event items.");
        }

        // Restore expected customers count/UI
        if (Expected_Customers_UI.Instance != null && Day_Plan_Manager.instance != null)
        {
            int planned = Day_Plan_Manager.instance.customersPlannedForEvening;
            Expected_Customers_UI.Instance.ShowExpectedCustomerCount(planned, animate: false);
            Debug.Log($"[Save_Manager] Refreshed Expected Customers UI after load: {planned}");
        }

        // Restore cutscene data
        if (Cutscene_Manager.Instance != null)
            Cutscene_Manager.Instance.LoadFromSave(currentGameData);


        // Handle room loading
        string roomKey = string.IsNullOrEmpty(currentGameData.currentRoom) 
            ? "Bedroom" // fallback room ID
            : currentGameData.currentRoom;

        if (RoomManager.RoomDictionary.TryGetValue(roomKey, out var targetRoom))
            StartCoroutine(LoadRoomScene(targetRoom));
        else
            Debug.LogWarning($"Room '{roomKey}' not found in RoomDictionary. Available keys: {string.Join(", ", RoomManager.RoomDictionary.Keys)}");
    }
    
    public void AutoSave()
    {
        SaveGameData(currentSaveSlot);
        Debug.Log("Auto-save completed.");
    }

    // Coroutine to handle scene loading
    private IEnumerator LoadRoomScene(Room_Data targetRoom)
    {
        // Use persistent screen fade (should exist in your initial scene)
        screenFade = Grimoire.Screen_Fade.instance;
        if (screenFade != null)
        {
            // Fade to black before loading
            yield return screenFade.StartCoroutine(screenFade.BlackFadeIn());
        }
        else
        {
            Debug.LogWarning("[Save_Manager] No persistent Screen_Fade found. Continuing without fade.");
        }

        // Load the target scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetRoom.roomID.ToString());
        while (!asyncLoad.isDone)
            yield return null;

        // Wait one frame for objects to initialize
        yield return null;

        // --- Player reposition logic ---
        if (targetRoom.isOverworldScene)
        {
            var player = FindObjectOfType<Player_Controller>();
            if (player == null)
                Debug.LogWarning($"[Save_Manager] No Player_Controller found in overworld scene {targetRoom.roomID}.");
            else
            {
                Transform spawnPoint = FindSpawnPointByID(Room_Data.SpawnPointID.Default);
                if (spawnPoint != null)
                {
                    player.transform.position = spawnPoint.position;
                    Debug.Log($"[Save_Manager] Player moved to spawn point '{spawnPoint.name}' in scene {targetRoom.roomID}.");
                }
            }
        }

       // --- MUSIC --- 
        if (targetRoom.music != null)
            Music_Persistence.instance.CheckMusic(targetRoom.music, targetRoom.musicVolume);
        else
            Music_Persistence.instance.StopMusic();

        // --- AMBIENT ---
        if (targetRoom.ambientSound != null)
            Music_Persistence.instance.CheckAmbient(targetRoom.ambientSound, targetRoom.ambientVolume);
        else
            Music_Persistence.instance.StopAmbient();

        // --- Weather ---
        if (Weather_Manager.Instance != null)
        {
            Weather_Manager.Instance.isRaining = currentGameData.isRaining;
            Weather_Manager.Instance.ApplyWeatherForCurrentScene();
        }

        // --- HOLD black for UI to finish hiding ---
        float uiHideDelay = 1.0f;
        yield return new WaitForSeconds(uiHideDelay);

        // Fade back into the scene
        if (screenFade != null)
        {
            // ensure alpha is 1 before fading out (should be if persistent)
            screenFade.fadeCanvasGroup.alpha = 1f;
            yield return screenFade.StartCoroutine(screenFade.BlackFadeOut());
        }
    }

    private Transform FindSpawnPointByID(Room_Data.SpawnPointID id)
    {
        string spawnName = $"{id}SpawnPoint";  // e.g., "DefaultSpawnPoint"
        GameObject spawnObj = GameObject.Find(spawnName);
        return spawnObj != null ? spawnObj.transform : null;
    }

    private string GetFilePath(int slot) => Path.Combine(Application.persistentDataPath, $"slot{slot}.json");
}

#region GameData + RoomManager
/// <summary>
/// Class to hold all game data for saving/loading
/// ONLY is used for saving global game data, not runtime data
/// </summary>
[System.Serializable]
public class GameData
{
    public float elapsedTime = 0f;
    public string currentRoom = "";
    public PlayerProgressData playerProgress;
    public Quest_Manager_Data questData;
    public IngredientInventoryData ingredientInventoryData;
    public DishInventoryData dishInventoryData;
    public RestaurantStateData restaurantStateData;
    public DailyMenuData dailyMenuData;
    public bool isRaining = false;
    public Day_Turnover_Manager.WeekDay currentDay = Day_Turnover_Manager.WeekDay.Monday;
    public List<AffectionEventItemsSaveData> affectionEventItems = new();
    public ChestSaveData chestData;
    public List<string> playedCutscenes = new List<string>();
}

/// <summary>
/// Static manager for room data and lookups
/// </summary>
[System.Serializable]
public static class RoomManager
{
    public static Dictionary<string, Room_Data> RoomDictionary;

    public static void Initialize(Room_Collection_Data collection)
    {
        RoomDictionary = new Dictionary<string, Room_Data>();

        foreach (var room in collection.rooms)
        {
            if (!RoomDictionary.ContainsKey(room.roomID.ToString()))
                RoomDictionary.Add(room.roomID.ToString(), room);
            else
                Debug.LogError($"Duplicate room name detected: {room.roomID}");
        }

        Debug.Log($"RoomManager initialized with {RoomDictionary.Count} rooms.");
    }
    #endregion
}

[System.Serializable]
public class RestaurantStateData
{
    public List<Customer_State> customers = new List<Customer_State>();
}

[System.Serializable]
public class DailyMenuData
{
    public List<Dish_Data.Dishes> dailyPool = new();
    public List<Dish_Data.Dishes> dishesSelected = new();
    public int customersPlanned = 0;
}

[System.Serializable]
public class AffectionEventItemsSaveData
{
    public string npcID;
    public string itemName;
    public bool unlocked;
    public bool collected;
}

[System.Serializable]
public class ChestSaveData
{
  public List<ChestItemData> itemsInChest = new();
}

[System.Serializable]
public class ChestItemData
{
    public ItemCategory category;
    public Dish_Data.Dishes dishType; // only used if category is Dish
    public IngredientType ingredientType; // only used if category is Ingredient
    public int amount;
}

public enum ItemCategory
{
  Dish,
  Ingredient
}




