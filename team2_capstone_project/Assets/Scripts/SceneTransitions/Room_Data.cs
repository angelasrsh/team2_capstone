using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rooms/Room", fileName = "NewRoom")]
public class Room_Data : ScriptableObject
{
  public enum RoomID
  {
    // room ID names must match scene names exactly
    Main_Menu,
    World_Map,
    Restaurant,
    Bedroom,
    Cooking_Minigame,
    Chopping_Minigame,
    Frying_Pan_Minigame,
    Combine_Minigame,
    Foraging_Area_Whitebox,
    Updated_Restaurant,
    Outside_Restaurant
  }

  public enum SpawnPointID
  {
    Default,
    Cauldron,
    Cutting_Board,
    Frying_Pan,
    Combine_Table,
    Door,
    Carriage
  }

  [Header("Room Info")]
  public RoomID roomID;
  public bool isOverworldScene;

  [Header("Audio Settings")]
  public AudioClip music;
  public AudioClip ambientSound;
  [Range(0f, 1f)] public float musicVolume = 1f;
  [Range(0f, 1f)] public float ambientVolume = 1f;

  [Header("Weather Settings")]
  public bool isOutdoorScene = true;  // whether to show/hide rain visuals
  public AudioClip rainMusic;
  [Range(0f, 1f)] public float rainMusicVolume = 1f;
  public AudioClip rainAmbient;
  [Range(0f, 1f)] public float rainAmbientVolume = 1f;

  [Header("Room Exits")]
  public RoomExitOptions[] exits;
}

[System.Serializable]
public class RoomExitOptions
{
  public Room_Data.RoomID exitingTo;
  public Room_Data.SpawnPointID spawnPointID;
  public Room_Data targetRoom;
  public bool overrideTimeOfDay;
  public Day_Turnover_Manager.TimeOfDay overrideValue;
}

