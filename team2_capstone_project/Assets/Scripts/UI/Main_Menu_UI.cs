using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Main_Menu_UI : MonoBehaviour
{
    [Header("Scene + Player References")]
    public Room_Data currentRoom;
    public Room_Data.RoomID exitingTo;
    private Player_Controller player;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private GameObject namePromptPanel;

    private bool isSceneTransitioning = false;
    private int currentSlot = 1;
    private InputAction confirmAction;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRebind;
        TryBindInput();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRebind;
        TryBindInput();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedRebind;
        if (confirmAction != null)
            confirmAction.performed -= OnConfirmPerformed;
    }

    private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode) => TryBindInput();
    private void TryBindInput()
    {
        PlayerInput playerInput = null;

        // Prefer Game_Manager if it holds PlayerInput
        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        // Fallback to Player_Input_Controller if needed
        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[Main_Menu_UI] No PlayerInput found in scene to bind actions.");
            return;
        }

        // Bind Interact action
        confirmAction = playerInput.actions["Confirm"];
        if (confirmAction == null)
            Debug.LogWarning("[Main_Menu_UI] Could not find 'Confirm' action in PlayerInput!");
        else
        {
            confirmAction.Enable();
            confirmAction.performed += OnConfirmPerformed;
            Debug.Log("[Main_Menu_UI] 'Confirm' action bound and enabled.");
        }
    }

    private void Start() => CheckForExistingSave();

    private void CheckForExistingSave()
    {
        string path = Path.Combine(Application.persistentDataPath, $"slot{currentSlot}.json");
        bool saveExists = File.Exists(path);

        loadGameButton.interactable = saveExists;
        Debug.Log(saveExists ? "Save file found, enabling Load Game." : "No save file found, disabling Load Game.");
    }

    public void OnNewGameButtonPressed() => namePromptPanel.SetActive(true);
    public void OnConfirmNewGame()
    {
        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Chef";

        // Set the player name in Player_Progress
        Player_Progress.Instance.SetPlayerName(playerName);

        // Create a new save slot (overwriting existing one)
        Save_Manager.instance.CreateNewSaveSlot(currentSlot);
        Save_Manager.instance.SaveGameData(currentSlot);

        Debug.Log($"New game created with player name: {playerName}");

        StartCoroutine(StartNewGameScene());
    }

    private void OnConfirmPerformed(InputAction.CallbackContext ctx)
    {
        if (isSceneTransitioning) return;
        
        if (namePromptPanel.activeSelf)
            OnConfirmNewGame();
        Debug.Log("Confirm pressed, confirming new game.");
    }

    public void OnBackButtonPressed() => namePromptPanel.SetActive(false);
    public void OnLoadGameButtonPressed()
    {
        if (isSceneTransitioning) return;

        Save_Manager.instance.LoadGameData(currentSlot);
        Debug.Log("Loaded game from existing save slot.");
    }

    private IEnumerator StartNewGameScene()
    {
        if (isSceneTransitioning) yield break;
        isSceneTransitioning = true;

        yield return new WaitForSeconds(0.3f);  // small delay for UI feel
        Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
    }
}
