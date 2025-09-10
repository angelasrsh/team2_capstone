using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rooms/Room", fileName = "NewRoom")]
public class RoomData : ScriptableObject
{
    public string roomName;
    public AudioClip music;
    public AudioClip ambientSound;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 1f;
    public string spawnPointName;
    public RoomExitOptions[] exits;
}

[CreateAssetMenu(menuName = "Rooms/RoomCollection", fileName = "NewRoomCollection")]
public class RoomCollectionData : ScriptableObject
{
    public List<RoomData> rooms;
}


[System.Serializable]
public class RoomExitOptions
{
    public string exitingTo;
    public string spawnPointName;
    public RoomData targetRoom;
}


