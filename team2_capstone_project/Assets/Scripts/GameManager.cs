using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

[System.Serializable]
public static class RoomManager
{
    public static Dictionary<string, RoomData> RoomDictionary;

    public static void Initialize(RoomCollectionData collection)
    {
        RoomDictionary = new Dictionary<string, RoomData>();

        foreach (var room in collection.rooms)
        {
            if (!RoomDictionary.ContainsKey(room.roomName))
            {
                RoomDictionary.Add(room.roomName, room);
            }
            else
            {
                Debug.LogError($"Duplicate room name detected: {room.roomName}");
            }
        }
        Debug.Log($"RoomManager initialized with {RoomDictionary.Count} rooms.");
    }
}
