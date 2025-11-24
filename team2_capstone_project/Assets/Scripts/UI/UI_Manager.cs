using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;
    private PlayerInput playerInput;
    public int openUICount = 0;
    public bool pauseMenuOn = false;
    private float inputCooldown = 0f;
    [SerializeField] private float uiInputCooldownDuration = 0.25f; // tweakable delay

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (inputCooldown > 0f)
            inputCooldown -= Time.unscaledDeltaTime;
    }

    private void Start()
    {
        playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
        if (playerInput == null)
            Debug.Log("[UI_Manager]: Player Input is null!");
    }

    private void OnEnable()
    {
        if (playerInput == null)
            playerInput = Game_Manager.Instance?.GetComponent<PlayerInput>();
    }

    public bool CanProcessInput()
    {
        return inputCooldown <= 0f;
    }

    public void PauseMenuState(bool paused)
    {
        pauseMenuOn = paused;
    }

    public void OpenUI()
    {
        // playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
        openUICount++;
        Debug.Log("[UI_Manager]: Opening some UI. Currently open: " + openUICount);
        if (playerInput != null && playerInput.currentActionMap.name != "UI")
        {
            playerInput.SwitchCurrentActionMap("UI");
            // Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;
        }

        // Rebind Talk Action
        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
        if (dm == null)
            Debug.Log("Cannot bind talkAction: no dialogue manager found.");
        else
            dm.BindTalkAction();
    }

    public void CloseUI()
    {
        // playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
        if (openUICount != 0)
            openUICount--;
        Debug.Log("[UI_Manager]: Closing some UI. Currently open: " + openUICount);
        if (playerInput != null && openUICount == 0)
        {
            playerInput.SwitchCurrentActionMap("Player");
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;

            // Start input cooldown after returning to Player map
            inputCooldown = uiInputCooldownDuration;
        }

        // Rebind Talk Action
        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
        if (dm == null)
            Debug.Log("Cannot bind talkAction: no dialogue manager found.");
        else
            dm.BindTalkAction();
    }
}
