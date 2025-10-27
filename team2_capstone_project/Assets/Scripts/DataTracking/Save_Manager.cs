using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class Save_Manager : MonoBehaviour
{
    public static Save_Manager instance;
    private static GameData currentGameData;
    public Room_Collection_Data roomCollection;
    private RoomExitOptions roomExitOptions;

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

    public void CreateNewSaveSlot(int slot)
    {
        if (currentGameData == null) // Only create if no active save data
        {
            currentGameData = new GameData
            {
                elapsedTime = tempElapsedTime
            };

            SetCurrentSlot(slot); // Update the current save slot
            SaveGameData(slot);   // Save the data to disk

            Debug.Log($"New save slot {slot} created with elapsed time: {tempElapsedTime}");
        }
        else
        {
            Debug.LogWarning($"Save slot {slot} already exists or is being used.");
        }
    }

    private void UpdateGameData()
    {
        var player = FindObjectOfType<Player_Controller>();
        if (player != null && player.currentRoom != null)
        {
            currentGameData.currentRoom = player.currentRoom.roomID.ToString();
            Debug.Log($"Updated current room: {currentGameData.currentRoom}");
        }

        currentGameData.playerProgress = Player_Progress.Instance.GetSaveData();

        // Save quest data
        if (Quest_Manager.Instance != null)
            currentGameData.questData = Quest_Manager.Instance.GetSaveData();

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

        // Handle room loading
        if (RoomManager.RoomDictionary.TryGetValue(currentGameData.currentRoom, out var targetRoom))
            StartCoroutine(LoadRoomScene(targetRoom));
        else
            Debug.LogWarning($"Room '{currentGameData.currentRoom}' not found in RoomDictionary.");
    }
    
    public void AutoSave()
    {
        SaveGameData(currentSaveSlot);
        Debug.Log("Auto-save completed.");
    }

    // Coroutine to handle scene loading
    private IEnumerator LoadRoomScene(Room_Data targetRoom)
    {
        // Load target scene asynchronously
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetRoom.roomID.ToString());

        // Wait until the scene is loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Find player in the new scene and position them at the spawn point
        var player = FindObjectOfType<Player_Controller>();
        if (player != null)
        {
            var spawnPoint = GameObject.Find(roomExitOptions.spawnPointID.ToString());
            if (spawnPoint != null)
            {
                player.transform.position = spawnPoint.transform.position;
                Debug.Log($"Player moved to spawn point: {roomExitOptions.spawnPointID} in scene {targetRoom.roomID}");
            }
            else
            {
                Debug.LogWarning($"Spawn point {roomExitOptions.spawnPointID} not found in scene {targetRoom.roomID}.");
            }
        }
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



