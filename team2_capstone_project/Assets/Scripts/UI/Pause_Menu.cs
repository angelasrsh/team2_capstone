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

  [Header("Player Input Info")]
  private InputAction pauseAction;
  private InputAction closeAction;
  private PlayerInput playerInput;

  private void Awake()
  {
    instance = this;
    HideMenu();
  }
  
  private void Start()
  {
    playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

    if (playerInput == null)
    {
      Debug.Log("[Pause_Menu]: PlayerInput component not found on Game_Manager!");
      return;
    }
    
    pauseAction = playerInput.actions["Pause"];
    closeAction = playerInput.actions.FindAction("CloseRegular", true);

    if (pauseAction != null)
      pauseAction.performed += ctx =>
      {
        if (canPause)
          PauseGame();
      };
    
    if (closeAction != null)
      closeAction.performed += ctx => {
        if (isPaused)
          ResumeGame();
      };
  }

  private void Update()
  {
    if (!canPause)
      return; // ignore pause if disabled
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
    {
      Debug.Log("Menubox is null?");
      return;
    }

    menuBox.SetActive(true);
    if (darkOverlay == null)
    {
      Debug.Log("Dark Overlay is null?");
      return;
    }

    darkOverlay.SetActive(true);
    isPaused = true;
    playerInput.SwitchCurrentActionMap("UI");
  }

  // Resume the game from the pause menu
  public void ResumeGame()
  {
    Audio_Manager.instance.PlaySFX(Audio_Manager.instance.menuClose);

    Debug.Log("Resuming game...");
    menuBox.SetActive(false);
    darkOverlay.SetActive(false);
    isPaused = false;
    playerInput.SwitchCurrentActionMap("Player");
  }

  // Quit the game from pause menu and return to the main menu
  public void QuitGame()
  {
    Debug.Log("Quitting game...");
    playerInput.SwitchCurrentActionMap("Player");
    Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
  }
  
  private void HideMenu()
  {
    menuBox.SetActive(false);
    darkOverlay.SetActive(false);
    isPaused = false;
  }
}
