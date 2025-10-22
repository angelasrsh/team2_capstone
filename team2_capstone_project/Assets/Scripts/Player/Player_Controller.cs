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

    [Header("Stamina Sprint")]
    public float sprintMultiplier = 1.8f;      
    public float maxStamina = 5f;  // seconds spent sprinting
    public float staminaRechargeDelay = 3f;  // seconds to wait before starting recharge
    public float staminaRechargeRate = 1f;  // stamina per second recharged
    [SerializeField] private Player_Stamina_UI staminaUI;

    // Internal stamina variables
    private float currentStamina;
    private bool isSprinting = false;
    private bool isRecharging = false;

    [Header("Mobile Input Deadzones")]

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
    private bool movementLocked = false;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;
    private bool isGrounded;
    private CharacterController controller;

    // Input System
    private PlayerInput playerInput;
    private InputAction moveAction, interactAction, openInventoryAction, openJournalAction, sprintAction;

    // Room tracking 
    [HideInInspector] public Room_Data currentRoom;

    // Internal movement vars
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
        sprintAction = playerInput.actions["Sprint"];

        if (moveAction != null)
            moveAction.performed += onMove;

        // Initialize stamina
        currentStamina = maxStamina;
        SendStaminaProgress();  

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
        HandleSprint();

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
        if (movementLocked || controller == null)
        return;

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


        // --- Sprinting ---
        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);


        // --- Acceleration / Deceleration --- //
        Vector3 targetVelocity = move * speed;

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

    public bool IsMoving() => movement.magnitude > 0.1f;

    /// <summary>
    /// Called by the input action when the player moves.
    /// Tell the GameEventsManager that this action occurred.
    /// GameEvents wants to know this for the sake of the tutorial.
    /// </summary>
    private void onMove(InputAction.CallbackContext context) => Game_Events_Manager.Instance?.PlayerMoved();

    #region Stamina Sprint
    private void HandleSprint()
    {
        bool sprintHeld = sprintAction.ReadValue<float>() > 0.5f;

        if (sprintHeld && currentStamina > 0 && !isRecharging)
        {
            isSprinting = true;
            Debug.Log("Sprinting");
            currentStamina -= Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                StartCoroutine(RechargeStamina());
            }
        }
        else
        {
            isSprinting = false;

            if (currentStamina < maxStamina && !isRecharging)
            {
                StartCoroutine(RechargeStamina());
            }
        }

        SendStaminaProgress();
    }

    private IEnumerator RechargeStamina()
    {
        if (isRecharging) yield break;

        isRecharging = true;
        yield return new WaitForSeconds(staminaRechargeDelay);

        while (currentStamina < maxStamina)
        {
            currentStamina += Time.deltaTime * staminaRechargeRate;
            SendStaminaProgress();
            yield return null;
        }

        currentStamina = maxStamina;
        isRecharging = false;
        SendStaminaProgress();
    }

    private void SendStaminaProgress()
    {
        float progress = currentStamina / maxStamina;
        staminaUI?.SetStamina(progress);
    }
    #endregion

    #region Various Helpers
    public void DisablePlayerMovement()
    {
        movementLocked = true;
        if (playerInput != null && moveAction != null)
        {
            moveAction.Disable();
            targetInput = Vector2.zero;
            smoothInput = Vector2.zero;
            currentMoveVelocity = Vector3.zero;
        }
    }

    public void EnablePlayerMovement()
    {
        movementLocked = false;
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
    #endregion
}
