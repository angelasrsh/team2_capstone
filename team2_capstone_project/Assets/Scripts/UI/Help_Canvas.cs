using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Help_Canvas : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject buttonPanel;


    private bool isPaused = false;
    [HideInInspector] public bool canPause = true;

    private PlayerInput playerInput;
    private InputAction pauseAction;

    private void Start()
    {
       
        playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("[Pause_Menu] No PlayerInput component found on Game_Manager in scene: " + SceneManager.GetActiveScene().name);
            return;
        }

        if (playerInput.actions == null)
        {
            Debug.LogError("[Pause_Menu] PlayerInput.actions is NULL — check that your Input Actions asset is assigned in the PlayerInput component!");
            return;
        }

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
        if (helpPanel == null)
            Debug.LogWarning("[Pause_Menu] helpPanel is null!");
        if (darkOverlay == null)
            Debug.LogWarning("[Pause_Menu] darkOverlay is null!");

        helpPanel?.SetActive(true);
        darkOverlay?.SetActive(true);
        buttonPanel?.SetActive(false);
        UI_Manager.Instance.PauseMenuState(true);
        isPaused = true;

        Time.timeScale = 0f;  // Pause game time
    }

    public void ResumeGame()
    {
        Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.menuClose);

        Debug.Log("Resuming game...");
        helpPanel?.SetActive(false);
        darkOverlay?.SetActive(false);
        buttonPanel?.SetActive(true);
        isPaused = false;
        UI_Manager.Instance.PauseMenuState(false);

        Time.timeScale = 1f;  // Resume game time
    }


    private void HideMenu()
    {
        helpPanel?.SetActive(false);
        darkOverlay?.SetActive(false);
        isPaused = false;
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;
    }
}
