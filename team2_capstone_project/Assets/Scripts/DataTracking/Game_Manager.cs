using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance;

    [Header("Databases")]
    [SerializeField] public Dish_Database dishDatabase;
    [SerializeField] public Foraging_Database foragingDatabase;
    [SerializeField] public NPC_Database npcDatabase;
    [SerializeField] private Player_Progress playerProgress;

    [Header("Room Setup")]
    [SerializeField] private Room_Collection_Data roomCollection;

    private void Awake()
    {
        if (roomCollection != null)
        {
            Room_Manager.Initialize(roomCollection);
            Debug.Log("GameManager: Room_Manager initialized!");
        }
        else
        {
            Debug.LogError("GameManager: No RoomCollectionData assigned!");
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (dishDatabase == null)
                Debug.LogError("GameManager: DishDatabase not set in inspector!");

            if (npcDatabase == null)
                Debug.LogError("GameManager: npcDatabase not set in inspector!");

            playerProgress.UnlockDish(Dish_Data.Dishes.Blinding_Stew);
            playerProgress.UnlockDish(Dish_Data.Dishes.Mc_Dragons_Burger);
            playerProgress.UnlockNPC(CustomerData.NPCs.Elf);
            playerProgress.UnlockIngredient(Ingredient_Inventory.Instance.IngrEnumToData(IngredientType.Bone));
            playerProgress.UnlockIngredient(Ingredient_Inventory.Instance.IngrEnumToData(IngredientType.Uncut_Fogshroom));
            playerProgress.UnlockIngredient(Ingredient_Inventory.Instance.IngrEnumToData(IngredientType.Uncut_Fermented_Eye));
            Debug.Log("GameManager: Player_Progress initialized. Blinding Stew, Mc_Dragons_Burger, Bone, Fogshroom, Fermented Eye, and Asper_Agis unlocked.");

            if (foragingDatabase == null)
                Debug.LogError("GameManager: ForagingDatabase not set in inspector!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame button pressed!");

    #if UNITY_EDITOR
        // Stop play mode if running in the editor
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        // Quit game if built
        Application.Quit();
    #endif
    }
}

[System.Serializable]
public static class Room_Manager
{
    public static Dictionary<Room_Data.RoomID, Room_Data> RoomDictionary;

    public static void Initialize(Room_Collection_Data collection)
    {
        RoomDictionary = new Dictionary<Room_Data.RoomID, Room_Data>();

        foreach (var room in collection.rooms)
        {
            if (!RoomDictionary.ContainsKey(room.roomID))
            {
                RoomDictionary.Add(room.roomID, room);
            }
            else
            {
                Debug.LogError($"Duplicate RoomID detected: {room.roomID}");
            }
        }

        Debug.Log($"RoomManager initialized with {RoomDictionary.Count} rooms.");
    }

    public static Room_Data GetRoom(Room_Data.RoomID id)
    {
        if (RoomDictionary == null)
        {
            Debug.LogError("RoomManager not initialized! Call Initialize() first.");
            return null;
        }

        if (RoomDictionary.TryGetValue(id, out var room))
        {
            return room;
        }

        Debug.LogError($"RoomID {id} not found in RoomManager.");
        return null;
    }

    /// <summary>
    /// Finds the Room_Data for the currently active scene
    /// Scene name must match a RoomID enum value exactly
    /// </summary>
    public static Room_Data GetRoomFromActiveScene()
    {
        if (RoomDictionary == null)
        {
            Debug.LogError("RoomManager not initialized! Call Initialize() first.");
            return null;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;

        if (System.Enum.TryParse(activeSceneName, out Room_Data.RoomID roomID))
        {
            return GetRoom(roomID);
        }

        Debug.LogError($"RoomManager: Active scene '{activeSceneName}' does not match any RoomID.");
        return null;
    }
}
