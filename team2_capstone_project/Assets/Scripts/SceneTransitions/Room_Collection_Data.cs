using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Rooms/RoomCollection", fileName = "NewRoomCollection")]
public class Room_Collection_Data : ScriptableObject
{
    public List<Room_Data> rooms;
}