using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player_Controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Tooltip("Deadzone to ENTER movement (larger).")]
    [Range(0f, 0.5f)]
    public float deadzoneEnter = 0.12f;

    [Tooltip("Deadzone to EXIT movement (smaller).")]
    [Range(0f, 0.25f)]
    public float deadzoneExit = 0.05f;

    [Tooltip("Smoothing time in seconds for input smoothing. Lower = snappier.")]
    [Range(0.01f, 0.2f)]
    public float smoothingTime = 0.06f;

    [Tooltip("Time in seconds to ignore brief zero inputs (helps with gamepad stick jitter).")]
    [SerializeField] private float zeroToleranceTime = 0.08f; // seconds allowed for a "fake zero"

    [HideInInspector] public Vector2 movement;

    // Input System
    private PlayerInput playerInput;
    private InputAction moveAction, interactAction, openInventoryAction, openJournalAction;

    // Room tracking 
    [HideInInspector] public Room_Data currentRoom;

    // Internal
    private Rigidbody rb;
    private Vector2 rawInput = Vector2.zero;
    private Vector2 targetInput = Vector2.zero;
    private Vector2 smoothInput = Vector2.zero;
    private Vector2 smoothVelocity = Vector2.zero;
    private Vector2 lastNonZeroInput = Vector2.zero;
    private Vector2 lastStableInput = Vector2.zero;
    private float zeroTimer = 0f;
    private bool onMobile = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null && rb.interpolation == RigidbodyInterpolation.None)
            rb.interpolation = RigidbodyInterpolation.Interpolate;

        playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("[Player_Controller] No PlayerInput found in scene. Make sure a PlayerInput component exists (GameManager).", this);
            return;
        }

        moveAction = playerInput.actions["Move"];
        interactAction = playerInput.actions["Interact"];
        openInventoryAction = playerInput.actions["OpenInventory"];
        openJournalAction = playerInput.actions["OpenJournal"];

        if (moveAction != null)
            moveAction.performed += onMove;

        // Detect if platform = mobile
        onMobile = Application.isMobilePlatform;
    }

    private void OnDestroy()
    {
        if (moveAction != null)
            moveAction.performed -= onMove;
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
    }

    private void Update()
    {
        Vector2 raw = moveAction.ReadValue<Vector2>();

        if (onMobile)
        {
            // --- MOBILE LOGIC (ignore single-frame zero drops) ---
            if (raw == Vector2.zero)
            {
                zeroTimer += Time.deltaTime;
                if (zeroTimer < zeroToleranceTime)
                {
                    raw = lastStableInput;
                }
                else
                {
                    lastStableInput = Vector2.zero;
                }
            }
            else
            {
                lastStableInput = raw;
                zeroTimer = 0f;
            }

            targetInput = raw;
        }
        else
        {
            // --- PC / CONSOLE LOGIC ---
            targetInput = raw;
        }
    }

    private void FixedUpdate()
    {
        // Smooth input for both modes
        smoothInput = Vector2.Lerp(smoothInput, targetInput, Time.fixedDeltaTime * 15f);

        movement = smoothInput;

        if (rb != null)
        {
            Vector3 newVel = new Vector3(movement.x * moveSpeed, rb.velocity.y, movement.y * moveSpeed);
            rb.velocity = newVel;
        }
    }

    public bool IsMoving() => movement != Vector2.zero;

    public void DisablePlayerController()
    {
        Player_Input_Controller.instance.DisablePlayerInput();
        if (rb != null) rb.velocity = Vector3.zero;
    }

    public void EnablePlayerController()
    {
        Player_Input_Controller.instance.EnablePlayerInput();
    }

    public void UpdatePlayerRoom(Room_Data.RoomID newRoomID)
    {
        Room_Data newRoom = Room_Manager.GetRoom(newRoomID);
        if (newRoom != null)
        {
            currentRoom = newRoom;
        }
        else
        {
            Debug.LogWarning($"Room not found: {newRoomID}");
        }
    }

    /// <summary>
    /// Called by the input action when the player moves.
    /// Tell the GameEventsManager that this action occurred.
    /// GameEvents wants to know this for the sake of the tutorial.
    /// </summary>
    private void onMove(InputAction.CallbackContext context)
    {
        Debug.Log("[P_C] Player is moving");
        Game_Events_Manager.Instance.PlayerMoved();
    }
}
