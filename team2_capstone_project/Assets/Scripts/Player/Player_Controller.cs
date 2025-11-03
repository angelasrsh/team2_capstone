using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;

// [RequireComponent(typeof(Rigidbody))]
public class Player_Controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 15f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform spriteTransform;

    [Header("Stamina Sprint")]
    public float sprintMultiplier = 1.8f;
    public float maxStamina = 5f;  // seconds spent sprinting
    public float staminaRechargeDelay = 3f;  // seconds to wait before starting recharge
    public float staminaRechargeRate = 1f;  // stamina per second recharged
    private Player_Stamina_UI staminaUI;
    // private Impact_Lines_UI sprintImpactLinesUI;

    // Internal stamina variables
    private float currentStamina;
    [HideInInspector] public bool isSprinting = false;
    private bool staminaWasFull = true; // track full state for SFX trigger
    private bool staminaDepleted = false;
    private float rechargeTimer = 0f;

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
    [HideInInspector] public CharacterController controller;
    private bool movementLocked = false;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;
    private bool isGrounded;
    [HideInInspector] public string currentSurface = "Grass";

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
        currentStamina = maxStamina;
        SendStaminaProgress();
        onMobile = Application.isMobilePlatform;

        if (spriteTransform == null)
        {
            spriteTransform = transform.Find("Sprite");
        }

        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponentInChildren<SpriteRenderer>();
            animator = spriteTransform.GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        TryBindInput();

        if (staminaUI == null)
            staminaUI = FindObjectOfType<Player_Stamina_UI>();

        if (staminaUI != null)
            staminaUI.SetStamina(currentStamina / maxStamina);

        currentRoom = Room_Manager.GetRoomFromActiveScene();
        if (currentRoom != null)
            Debug.Log($"[Player_Controller] Player spawned in room: {currentRoom.roomID}");

        // sprintImpactLinesUI = GetComponentInChildren<Impact_Lines_UI>();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindInput();

        if (staminaUI == null)
            staminaUI = FindObjectOfType<Player_Stamina_UI>();

        if (staminaUI != null)
            staminaUI.SetStamina(currentStamina / maxStamina);

        // sprintImpactLinesUI = GetComponentInChildren<Impact_Lines_UI>();
    }

    private void TryBindInput()
    {
        var input = FindObjectOfType<PlayerInput>();
        if (input == null)
        {
            Debug.LogWarning("[Player_Controller] No PlayerInput found yet — will retry next frame.");
            StartCoroutine(WaitAndBindInput());
            return;
        }

        BindInputActions(input);
    }

    private IEnumerator WaitAndBindInput()
    {
        yield return null; // wait one frame for PlayerInput to spawn
        var input = FindObjectOfType<PlayerInput>();
        if (input != null)
            BindInputActions(input);
    }

    private void BindInputActions(PlayerInput input)
    {
        if (input == null || input.actions == null)
        {
            Debug.LogWarning("[Player_Controller] Tried to bind inputs but PlayerInput or its actions are null.");
            return;
        }

        // Unsubscribe previous bindings safely
        if (moveAction != null)
            moveAction.performed -= onMove;

        playerInput = input;

        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];

        moveAction?.Enable();
        sprintAction?.Enable();

        if (moveAction != null)
            moveAction.performed += onMove;

        playerInput.onActionTriggered -= HandleGlobalInputTriggered;
        playerInput.onActionTriggered += HandleGlobalInputTriggered;

        Debug.Log("[Player_Controller] Input actions bound successfully in scene: " + SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        Vector2 raw = moveAction.ReadValue<Vector2>();
        HandleSprint();

        // --- Sprint Lines Control ---
        // if (sprintImpactLinesUI != null)
        // {
        //     if (isSprinting && IsMoving())
        //         sprintImpactLinesUI.ShowLines();
        //     else
        //         sprintImpactLinesUI.HideLines();
        // }

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
            DetectSurface();

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

    private void LateUpdate()
    {
        transform.forward = Vector3.forward;
        HandleWalkAnimation();
    }

    private void HandleWalkAnimation()
    {
        if (animator == null || spriteRenderer == null)
            return;

        // Calculate current movement speed
        float horizontalSpeed = new Vector3(currentMoveVelocity.x, 0f, currentMoveVelocity.z).magnitude;

        if (animator.HasParameterOfType("speed", AnimatorControllerParameterType.Float))
            animator.SetFloat("speed", horizontalSpeed);

        // Flip the sprite based on horizontal input
        if (horizontalSpeed > 0.05f)
        {
            bool facingRight = movement.x > 0.01f;
            spriteRenderer.flipX = facingRight;

            if (animator.HasParameterOfType("facingRight", AnimatorControllerParameterType.Bool))
                animator.SetBool("facingRight", facingRight);
        }

        // Adjust animation playback speed based on sprint
        if (isSprinting)
        {
            float targetAnimSpeed = isSprinting ? 1.5f : 1f;
            animator.speed = Mathf.Lerp(animator.speed, targetAnimSpeed, Time.deltaTime * 8f);
        }
        else
            animator.speed = 1.0f; // normal speed when walking or idle
    }

    /// <summary>
    /// Called by the input action when the player moves.
    /// Tell the GameEventsManager that this action occurred.
    /// GameEvents wants to know this for the sake of the tutorial.
    /// </summary>
    private void onMove(InputAction.CallbackContext context) => Game_Events_Manager.Instance?.PlayerMoved();


    #region Material Detect
    private void DetectSurface()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down,
            out RaycastHit hit, groundCheckDistance + 0.3f, groundMask))
        {
            if (hit.collider.CompareTag("Grass"))
                currentSurface = "Grass";
            else if (hit.collider.CompareTag("Wood"))
                currentSurface = "Wood";
            else if (hit.collider.CompareTag("Stone"))
                currentSurface = "Stone";
            else
                currentSurface = "Default";
        }
        else
        {
            Debug.LogWarning("Player ground surface detection raycast did not hit any ground.");
        }
    }
    #endregion


    #region Stamina Sprint
    private void HandleSprint()
    {
        bool sprintHeld = sprintAction.ReadValue<float>() > 0.5f;
        bool isMoving = movement.sqrMagnitude > 0.01f;

        // --- Sprint conditions ---
        if (sprintHeld && isMoving && !staminaDepleted && currentStamina > 0f)
        {
            isSprinting = true;
            currentStamina -= Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

            if (Mathf.Approximately(currentStamina, 0f))
            {
                // Out of stamina — stop sprinting
                staminaDepleted = true;
                isSprinting = false;
                rechargeTimer = 0f;
            }
        }
        else
        {
            isSprinting = false;
        }

        // --- Recharge logic ---
        if (!isSprinting && currentStamina < maxStamina)
        {
            // Count delay if depleted
            if (staminaDepleted)
            {
                rechargeTimer += Time.deltaTime;
                if (rechargeTimer >= staminaRechargeDelay)
                    staminaDepleted = false;
            }

            // Start recharging only if delay elapsed
            if (!staminaDepleted)
            {
                currentStamina += Time.deltaTime * staminaRechargeRate;
                currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            }
        }

        // --- Track & UI ---
        bool staminaFullNow = Mathf.Approximately(currentStamina, maxStamina);
        staminaWasFull = staminaFullNow;

        SendStaminaProgress();
    }

    private void SendStaminaProgress()
    {
        if (staminaUI == null) return;

        float progress = currentStamina / maxStamina;
        staminaUI.SetStamina(progress);
    }

    public bool IsSprinting() => isSprinting;
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
            currentRoom = newRoom;
        else
            Debug.LogWarning($"Room not found: {newRoomID}");
    }

    private void HandleGlobalInputTriggered(InputAction.CallbackContext ctx)
    {
        // Get reference to PlayerInput if lost during scene load
        if (playerInput == null)
        {
            PlayerInput input = FindObjectOfType<PlayerInput>();
            if (input != null && input.actions != null)
            {
                Debug.Log("[Player_Controller] Reacquiring PlayerInput after scene load...");
                BindInputActions(input);
            }
        }
    }
    #endregion
}

