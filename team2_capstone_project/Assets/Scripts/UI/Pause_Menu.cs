using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Pause_Menu : MonoBehaviour
{
  public static Pause_Menu instance;

  [Header("UI Elements")]
  public GameObject menuBox;
  public GameObject darkOverlay;

  [Header("Room Change Settings")]
  public Room_Data currentRoom;
  public Room_Data.RoomID exitingTo;

  private bool isPaused = false;
  [HideInInspector] public bool canPause = true; 
  private InputAction pauseAction;

  private void Awake()
  {
    instance = this;
    HideMenu();
  }
  
  private void Start()
  {
    Player_Input_Controller pic = FindObjectOfType<Player_Input_Controller>();
    if (pic != null)
    {
      pauseAction = pic.GetComponent<PlayerInput>().actions["Pause"];
    }

    if (pauseAction != null)
      pauseAction.performed += ctx => {
          if (canPause) {
              if (isPaused)
                  ResumeGame();
              else
                  PauseGame();
          }
      };
  }

  private void Update()
  {
      if (!canPause) return; // ignore pause if disabled
  }

  public void SetCanPause(bool value)
  {
      canPause = value;

      if (!canPause && isPaused)
          ResumeGame(); // auto-resume if pause is forcibly disabled
  }

  public void PauseGame()
  {
    Audio_Manager.instance.PlaySFX(Audio_Manager.instance.menuOpen);

    Debug.Log("Pausing game...");
    if (menuBox == null)
      Debug.Log("Menubox is null?");
    menuBox.SetActive(true);
    if (darkOverlay == null)
      Debug.Log("Dark Overlay is null?");
    darkOverlay.SetActive(true);
    isPaused = true;
  }

  // Resume the game from the pause menu
  public void ResumeGame()
  {
    Audio_Manager.instance.PlaySFX(Audio_Manager.instance.menuClose);

    Debug.Log("Resuming game...");
    menuBox.SetActive(false);
    darkOverlay.SetActive(false);
    isPaused = false;
  }

  // Quit the game from pause menu and return to the main menu
  public void QuitGame()
  {
    Debug.Log("Quitting game...");
    Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
  }
  
  private void HideMenu()
  {
    menuBox.SetActive(false);
    darkOverlay.SetActive(false);
    isPaused = false;
  }
}
