using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Grimoire;

public class Room_Change_Manager : MonoBehaviour
{
  public static Room_Change_Manager instance;
  private Screen_Fade blackScreenFade;

  private void Awake()
  {
    if (instance == null)
    {
      instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }
    
  private void Start()
  {
    Room_Data startingRoom = Room_Manager.GetRoomFromActiveScene();
    if (startingRoom != null)
    {
      if (startingRoom.music != null)
        Music_Persistence.instance.CheckMusic(startingRoom.music, startingRoom.musicVolume);
      else
        Music_Persistence.instance.StopMusic();

      if (startingRoom.ambientSound != null)
        Music_Persistence.instance.CheckAmbient(startingRoom.ambientSound, startingRoom.ambientVolume);
      else
        Music_Persistence.instance.StopAmbient();
    }
  }

  // <<summary>
  // Call this to transition to a new room.
  // currentRoomID: the RoomID of the room you are currently in
  // exitingTo: the RoomID of the room you want to go to
  // e.g. Room_Change_Manager.instance.GoToRoom(Room_Data.RoomID.Restaurant, Room_Data.RoomID.CookingMinigame);
  // </summary>
  public void GoToRoom(Room_Data.RoomID currentRoomID, Room_Data.RoomID exitingTo)
  {
    Room_Data currentRoom = Room_Manager.GetRoom(currentRoomID);
    if (currentRoom == null)
    {
      Debug.LogError($"[Room_Change_Manager] Could not find current room: {currentRoomID}. " +
                  $"Check that Game_Manager is in the scene and RoomCollectionData includes this room.");
      return;
    }

    // Save state if leaving Restaurant
    if (currentRoomID == Room_Data.RoomID.Restaurant)
    {
      if (Restaurant_State.Instance != null)
      {
        Restaurant_State.Instance.SaveCustomers();
        Debug.Log("Room_Change_Manager: Saved Restaurant_State before transition.");
      }
      else
      {
        Debug.LogWarning("Room_Change_Manager: Restaurant_State.Instance is null when trying to save.");
      }
    }

    // Try to get the exit
    RoomExitOptions exit = Exit(currentRoom, exitingTo);
    if (exit == null)
    {
      Debug.LogError($"[Room_Change_Manager] No exit defined from {currentRoomID} to {exitingTo}. " +
                  $"Make sure {currentRoomID} has an exit in its Room_Data asset.");
      return;
    }

    // Verify target room is valid
    if (exit.targetRoom == null)
    {
      Debug.LogError($"[Room_Change_Manager] Exit from {currentRoomID} to {exitingTo} has no target room assigned!");
      return;
    }

    // Set time of day (and override if needed)
    if (Day_Turnover_Manager.Instance != null)
    {
        if (exit.overrideTimeOfDay)
        {
            Day_Turnover_Manager.Instance.SetTimeOfDay(exit.overrideValue);
        }
        else
        {
            // fallback: target room could have a default if you want
            Debug.Log($"No time override for this exit. Using whatever state already exists: {Day_Turnover_Manager.Instance.currentTimeOfDay}");
        }
    }
    Game_Events_Manager.Instance.RoomChange(currentRoomID, exitingTo);
    StartCoroutine(HandleRoomTransition(exit.targetRoom, exit.spawnPointID));
  }

  private RoomExitOptions Exit(Room_Data room, Room_Data.RoomID exitingTo)
  {
    foreach (var exit in room.exits)
    {
      if (exit.exitingTo == exitingTo)
        return exit;
    }
    return null;
  }

  // <<summary>
  // Handles the transition process: fade out, load new scene, fade in.
  // </summary>
  private IEnumerator HandleRoomTransition(Room_Data targetRoom, Room_Data.SpawnPointID spawnPointID)
  {
    blackScreenFade = FindObjectOfType<Screen_Fade>();
    if (blackScreenFade == null)
    {
      Debug.LogWarning("blackScreenFade missing.");
      yield break;
    }

    // Disable player controls
    if (Player_Input_Controller.instance != null)
      Player_Input_Controller.instance.DisablePlayerInput();

    // Fade out music/ambient
    if (Music_Persistence.instance != null)
    {
      Music_Persistence.instance.PreTransitionCheckMusic(targetRoom.music);
      Music_Persistence.instance.PreTransitionCheckAmbient(targetRoom.ambientSound);
    }

    // Fade screen to black
    blackScreenFade.StartCoroutine(blackScreenFade.BlackFadeIn());
    yield return new WaitForSeconds(blackScreenFade.fadeDuration);

    // Load new room
    yield return StartCoroutine(ChangeRoom(targetRoom, spawnPointID));
  }

  private IEnumerator ChangeRoom(Room_Data targetRoom, Room_Data.SpawnPointID spawnPointID)
  {
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetRoom.roomID.ToString());
    while (!asyncLoad.isDone)
      yield return null;

    // place player immediately after scene loads prior to fade out
    if (targetRoom.isOverworldScene)
    {
      GameObject player = GameObject.FindWithTag("Player");
      if (player != null)
      {
        Spawn_Point[] spawnPoints = FindObjectsOfType<Spawn_Point>();
        foreach (var sp in spawnPoints)
        {
          if (sp.spawnPointID == spawnPointID)
          {
            player.transform.position = sp.transform.position;
            break;
          }
        }
      }
    }

    // now fade out
    blackScreenFade = FindObjectOfType<Screen_Fade>();
    if (blackScreenFade != null)
    {
      yield return StartCoroutine(HandleNewRoomTransition(targetRoom));
    }

    // re-enable player controls and set music/ambient
    if (Player_Input_Controller.instance != null)
      Player_Input_Controller.instance.EnablePlayerInput();

    if (targetRoom.music != null)
      Music_Persistence.instance.CheckMusic(targetRoom.music, targetRoom.musicVolume);
    else
      Music_Persistence.instance.StopMusic();

    if (targetRoom.ambientSound != null)
      Music_Persistence.instance.CheckAmbient(targetRoom.ambientSound, targetRoom.ambientVolume);
    else
      Music_Persistence.instance.StopAmbient();
  }

  private IEnumerator HandleNewRoomTransition(Room_Data targetRoom)
  {
    if (blackScreenFade == null)
      yield break;

    blackScreenFade.fadeCanvasGroup.alpha = 1;

    yield return blackScreenFade.StartCoroutine(blackScreenFade.BlackFadeOut());
  }
}
