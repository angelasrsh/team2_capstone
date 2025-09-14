using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rooms/Room", fileName = "NewRoom")]
public class Room_Data : ScriptableObject
{
    public enum RoomID
    {
        MainMenu,
        FirstResourceArea,
        Restaurant,
        CookingMinigame
    }

    public enum SpawnPointID
    {
        Default,
        Entrance,
        Exit
    }

    [Header("Room Info")]
    public RoomID roomID;
    public bool isOverworldScene;

    [Header("Audio Settings")]
    public AudioClip music;
    public AudioClip ambientSound;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 1f;

    [Header("Room Exits")]
    public RoomExitOptions[] exits;
}

[CreateAssetMenu(menuName = "Rooms/RoomCollection", fileName = "NewRoomCollection")]
public class RoomCollectionData : ScriptableObject
{
    public List<Room_Data> rooms;
}

[System.Serializable]
public class RoomExitOptions
{
    public Room_Data.RoomID exitingTo;
    public Room_Data.SpawnPointID spawnPointID;
    public Room_Data targetRoom;
}

