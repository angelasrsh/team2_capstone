using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Room Setup")]
    [SerializeField] private RoomCollectionData roomCollection;

    private void Awake()
    {
        if (roomCollection != null)
        {
            RoomManager.Initialize(roomCollection);
        }
        else
        {
            Debug.LogError("GameManager: No RoomCollectionData assigned!");
        }
    }
}

[System.Serializable]
public static class RoomManager
{
    public static Dictionary<RoomData.RoomID, RoomData> RoomDictionary;

    public static void Initialize(RoomCollectionData collection)
    {
        RoomDictionary = new Dictionary<RoomData.RoomID, RoomData>();

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

    public static RoomData GetRoom(RoomData.RoomID id)
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
