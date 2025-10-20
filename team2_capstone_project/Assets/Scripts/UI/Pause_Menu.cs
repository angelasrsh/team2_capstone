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

    private PlayerInput playerInput;
    private InputAction pauseAction;

    private void Awake()
    {
        instance = this;
        HideMenu();
    }

    private void Start()
    {
        if (Game_Manager.Instance == null)
        {
            Debug.LogError("[Pause_Menu] No Game_Manager found.");
            return;
        }

        playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("[Pause_Menu] No PlayerInput component on Game_Manager.");
            return;
        }

        // Bind pause action safely
        pauseAction = playerInput.actions.FindAction("Pause", true);
        if (pauseAction == null)
        {
            Debug.LogError("[Pause_Menu] Could not find 'Pause' action in PlayerInput actions.");
            return;
        }

        pauseAction.Enable();
        pauseAction.performed += OnPausePerformed;

        Debug.Log("[Pause_Menu] Bound to Pause action. Current map: " + playerInput.currentActionMap.name);
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (!canPause) return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void SetCanPause(bool value)
    {
        canPause = value;

        if (!canPause && isPaused)
            ResumeGame(); // auto-resume if pause forcibly disabled
    }

    public void PauseGame()
    {
        Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.menuOpen);

        Debug.Log("Pausing game...");
        if (menuBox == null)
            Debug.LogWarning("[Pause_Menu] menuBox is null!");
        if (darkOverlay == null)
            Debug.LogWarning("[Pause_Menu] darkOverlay is null!");

        menuBox?.SetActive(true);
        darkOverlay?.SetActive(true);
        UI_Manager.Instance.PauseMenuState(true);
        isPaused = true;

        Time.timeScale = 0f;  // Pause game time
    }

    public void ResumeGame()
    {
        Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.menuClose);

        Debug.Log("Resuming game...");
        menuBox?.SetActive(false);
        darkOverlay?.SetActive(false);
        isPaused = false;
        UI_Manager.Instance.PauseMenuState(false);

        Time.timeScale = 1f;  // Resume game time
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        ResumeGame();
        Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
    }

    private void HideMenu()
    {
        menuBox?.SetActive(false);
        darkOverlay?.SetActive(false);
        isPaused = false;
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;
    }
}
