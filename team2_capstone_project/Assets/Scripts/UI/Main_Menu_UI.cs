using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

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

    private void Start()
    {
        CheckForExistingSave();
    }

    private void CheckForExistingSave()
    {
        string path = Path.Combine(Application.persistentDataPath, $"slot{currentSlot}.json");
        bool saveExists = File.Exists(path);

        loadGameButton.interactable = saveExists;
        Debug.Log(saveExists ? "Save file found, enabling Load Game." : "No save file found, disabling Load Game.");
    }

    public void OnNewGameButtonPressed()
    {
        namePromptPanel.SetActive(true);
    }

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

        yield return new WaitForSeconds(0.3f); // small delay for UI feel
        Room_Change_Manager.instance.GoToRoom(currentRoom.roomID, exitingTo);
    }
}
