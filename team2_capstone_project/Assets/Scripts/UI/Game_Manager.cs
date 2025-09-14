using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Manager : MonoBehaviour
{
    [Header("Room Setup")]
    [SerializeField] private Room_Collection_Data roomCollection;

    private void Awake()
    {
        if (roomCollection != null)
        {
            Room_Manager.Initialize(roomCollection);
        }
        else
        {
            Debug.LogError("GameManager: No RoomCollectionData assigned!");
        }
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
}
