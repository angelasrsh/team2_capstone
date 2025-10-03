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

    // Internal
    private Rigidbody rb;
    private Vector2 rawInput = Vector2.zero;
    private Vector2 targetInput = Vector2.zero;
    private Vector2 smoothInput = Vector2.zero;
    private Vector2 smoothVelocity = Vector2.zero;
    private Vector2 lastNonZeroInput = Vector2.zero;
    private Vector2 lastStableInput = Vector2.zero;
    private float zeroTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Recommend interpolation for smoother visuals
        if (rb != null && rb.interpolation == RigidbodyInterpolation.None)
            rb.interpolation = RigidbodyInterpolation.Interpolate;

        // IMPORTANT: assign to the field (don't shadow)
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
            moveAction.performed += onMove; // keep tutorial hook behavior
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

        if (raw == Vector2.zero)
        {
            // Count how long we've been at zero
            zeroTimer += Time.deltaTime;

            // If it hasn't been zero long enough, keep the last stable input
            if (zeroTimer < zeroToleranceTime)
            {
                raw = lastStableInput;
            }
            else
            {
                // It's a real zero now, reset
                lastStableInput = Vector2.zero;
            }
        }
        else
        {
            // Got a real non-zero input, store it
            lastStableInput = raw;
            zeroTimer = 0f;
        }

        // Apply your deadzone/smoothing on top of 'raw'
        targetInput = raw;
    }

    private void FixedUpdate()
    {
        // Smooth the input so sudden sign swaps don't momentarily zero velocity
        smoothInput = Vector2.Lerp(smoothInput, targetInput, Time.fixedDeltaTime * 15f);

        // Expose movement for other systems
        movement = smoothInput;

        // Apply to rigidbody (preserve current Y velocity)
        if (rb != null)
        {
            Vector3 newVel = new Vector3(movement.x * moveSpeed, rb.velocity.y, movement.y * moveSpeed);
            rb.velocity = newVel;
        }
    }

    public bool IsMoving()
    {
        return movement != Vector2.zero;
    }

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
            // If you also want to store currentRoom on this script, add a field and assign here
        }
        else
        {
            Debug.LogWarning($"Room not found: {newRoomID}");
        }
    }

    private void onMove(InputAction.CallbackContext context)
    {
        // keep your tutorial/event behavior
        if (context.performed)
            Game_Events_Manager.Instance.PlayerMoved();
    }
}
