using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Mobile_Icon_UI : MonoBehaviour
{
    [Header("Main UI Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button journalButton;

    [Header("Close (X) Buttons")]
    [SerializeField] private Button closePauseButton;
    [SerializeField] private Button closeInventoryButton;
    [SerializeField] private Button closeJournalButton;

    public static Mobile_Icon_UI instance;
    private Journal_Menu journalMenu;
    private Canvas inventoryCanvas;

    private PlayerInput playerInput;
    private InputAction swipeAction;
    private Vector2 startTouchPos;
    private bool isTouching;

    private void Awake()
    {
        // Singleton pattern

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Hide automatically on non-mobile platforms
        bool simulateMobile = false;
        if (simulateMobile == false && SystemInfo.deviceType != DeviceType.Handheld)
        {
            Destroy(this.gameObject);
            return;
        }
        
        // #if UNITY_EDITOR
        //     simulateMobile = true; // comment this back in with the #if and #endif if you want to simulate mobile in editor
        // #endif

        if (SystemInfo.deviceType != DeviceType.Handheld && !simulateMobile)
        {
            gameObject.SetActive(false);
        }

        playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
            swipeAction = playerInput.actions["CloseInventory"];
    }

    private void Start()
    {
        // Ensure all close buttons start hidden
        closePauseButton.gameObject.SetActive(false);
        closeInventoryButton.gameObject.SetActive(false);
        closeJournalButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Ensure close buttons hidden at start
        closePauseButton?.gameObject.SetActive(false);
        closeInventoryButton?.gameObject.SetActive(false);
        closeJournalButton?.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandleSwipe();
    }

    /// <summary>
    /// Handles swipe input to close the inventory on mobile devices.
    /// </summary>
    private void HandleSwipe()
    {
        if (Touchscreen.current == null) return; // only process on mobile

        if (Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (!isTouching)
            {
                isTouching = true;
                startTouchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
        }
        else if (isTouching)
        {
            Vector2 endPos = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector2 delta = endPos - startTouchPos;

            // Check for left swipe
            if (delta.x < -100f && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                TryCloseInventory();
            }

            isTouching = false;
        }
    }

    private void TryCloseInventory()
    {
        if (inventoryCanvas != null && inventoryCanvas.enabled)
        {
            inventoryCanvas.enabled = false;
            Game_Events_Manager.Instance.InventoryToggled(false);
            EnableMainButtons();
            closeInventoryButton.gameObject.SetActive(false);
            Debug.Log("[Mobile_Icon_UI] Inventory closed via swipe.");
        }
    }

    #region Open Menu Button Events
    // ---------------------------------
    //  Menu Button Events
    // ---------------------------------
    public void OnSettingsButtonPressed()
    {
        Pause_Menu.instance.PauseGame();
        DisableMainButtons();
        closePauseButton.gameObject.SetActive(true);
    }

    public void OnInventoryButtonPressed()
    {
        inventoryCanvas = GameObject.Find("Canvas_Inventory_Open")?.GetComponent<Canvas>();
        if (inventoryCanvas == null)
        {
            Debug.LogWarning("[Mobile_Icon_UI] Error: no InventoryCanvas found!");
            return;
        }

        bool newState = !inventoryCanvas.enabled;
        inventoryCanvas.enabled = newState;
        Game_Events_Manager.Instance.InventoryToggled(newState);

        if (newState)
        {
            DisableMainButtons();
            closeInventoryButton.gameObject.SetActive(true);
        }
        else
        {
            EnableMainButtons();
            closeInventoryButton.gameObject.SetActive(false);
        }
    }

    public void OnJournalButtonPressed()
    {
        Canvas journalCanvas = GameObject.Find("Journal")?.GetComponent<Canvas>();
        if (journalCanvas == null)
        {
            Debug.LogWarning("[Mobile_Icon_UI] Error: no JournalCanvas found!");
            return;
        }

        journalMenu = journalCanvas.GetComponent<Journal_Menu>();
        bool isOpen = journalCanvas.enabled && journalMenu != null && journalMenu.gameObject.activeInHierarchy;

        if (!isOpen)
        {
            journalMenu.ResumeGame();
            Game_Events_Manager.Instance.JournalToggled(false);
            EnableMainButtons();
            closeJournalButton.gameObject.SetActive(false);
        }
        else
        {
            journalMenu.PauseGame();
            Game_Events_Manager.Instance.JournalToggled(true);
            DisableMainButtons();
            closeJournalButton.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Close (X) Button Events
    // ---------------------------------
    //  Close (X) Button Events
    // ---------------------------------
    public void OnClosePausePressed()
    {
        Pause_Menu.instance.ResumeGame();
        EnableMainButtons();
        closePauseButton.gameObject.SetActive(false);
    }

    public void OnCloseInventoryPressed()
    {
        if (GameObject.Find("Canvas_Inventory_Open") != null)
        {
            inventoryCanvas = GameObject.Find("Canvas_Inventory_Open").GetComponent<Canvas>();
            inventoryCanvas.enabled = false;
            Game_Events_Manager.Instance.InventoryToggled(false);
        }
        EnableMainButtons();
        closeInventoryButton.gameObject.SetActive(false);
    }

    public void OnInventoryCloseButtonPressed()
    {
        TryCloseInventory();
    }

    public void OnCloseJournalPressed()
    {
        if (journalMenu != null)
        {
            journalMenu.ResumeGame();
            Game_Events_Manager.Instance.JournalToggled(false);
        }
        EnableMainButtons();
        closeJournalButton.gameObject.SetActive(false);
    }
    #endregion

    // ---------------------------------
    //  Utility
    // ---------------------------------
    private void DisableMainButtons()
    {
        settingsButton.interactable = false;
        inventoryButton.interactable = false;
        journalButton.interactable = false;
    }

    private void EnableMainButtons()
    {
        settingsButton.interactable = true;
        inventoryButton.interactable = true;
        journalButton.interactable = true;
    }
}
