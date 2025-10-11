using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grimoire;

public class Room_Change_Trigger : MonoBehaviour
{
  public Room_Data currentRoom;
  public Room_Data.RoomID exitingTo;
  private Player_Controller player;

  private bool isSceneTransitioning = false;

  private void OnTriggerEnter(Collider other)
  {
    if (isSceneTransitioning) return;
    
    if (other.CompareTag("Player"))
    {
      Debug.Log("Player entered the trigger for room change: " + exitingTo);
      isSceneTransitioning = true;
      Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);    
    }
  }

  public void OnPlayButtonPressed()
  {
    if (isSceneTransitioning) return;
    isSceneTransitioning = true;

    Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
  }
  
  public void OnBackButtonPressedForMinigame()
  {
    if (isSceneTransitioning) return;
    isSceneTransitioning = true;
  
    Audio_Manager.instance.StopBubbling();
    Audio_Manager.instance.StopAmbientFire();
    Drag_All.ResetMinigame();
    Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
  }
}