using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.InputSystem;

// [RequireComponent(typeof(Rigidbody))]
public class Player_Controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 15f;

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

    [Header("Grounding / Slope Stick")]
    public float groundCheckDistance = 0.5f;  // how far below the player to check for ground
    public float slopeStickStrength = 8f;  // how strongly the player is pushed down slopes
    public LayerMask groundMask = ~0;

    [HideInInspector] public Vector2 movement;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;
    private bool isGrounded;
    private CharacterController controller;

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
        // rb = GetComponent<Rigidbody>();
        // if (rb != null && rb.interpolation == RigidbodyInterpolation.None)
        //     rb.interpolation = RigidbodyInterpolation.Interpolate;

        controller = GetComponent<CharacterController>();
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
        if (controller == null) return;

        // Smooth input for movement
        smoothInput = Vector2.MoveTowards(smoothInput, targetInput, 15f * Time.fixedDeltaTime);
        movement = smoothInput;

        // Check if grounded
        isGrounded = controller.isGrounded;

        // Handle gravity
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // small downward bias to stay grounded
        else
            velocity.y += Physics.gravity.y * Time.fixedDeltaTime;

        // Horizontal movement input
        Vector3 move = new Vector3(movement.x, 0f, movement.y);
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // --- Acceleration / Deceleration --- //
        Vector3 targetVelocity = move * moveSpeed;

        float rate = (move.sqrMagnitude > 0.01f) ? acceleration : deceleration;
        currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        // Combine movement + gravity
        Vector3 finalMove = currentMoveVelocity + velocity;
        controller.Move(finalMove * Time.fixedDeltaTime);

        // --- Extra slope sticking (for going down slopes) ---
        if (isGrounded)
        {
            // Cast slightly below player
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance,
                groundMask))
            {
                float groundAngle = Vector3.Angle(hit.normal, Vector3.up);

                if (groundAngle > 0.1f)
                {
                    // Push straight down (world Y) instead of along slope normal
                    Vector3 stickDir = Vector3.down * slopeStickStrength * Time.fixedDeltaTime;
                    controller.Move(stickDir);
                }
            }
        }
    }

    public bool IsMoving()
    {
        return movement.magnitude > 0.1f;
    }

    public void DisablePlayerMovement()
    {
        if (playerInput != null && moveAction != null)
            moveAction.Disable();
    }

    public void EnablePlayerMovement()
    {
        if (playerInput != null && moveAction != null)
            moveAction.Enable();
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
        // Debug.Log("[P_C] Player is moving");
        Game_Events_Manager.Instance.PlayerMoved();
    }
}
