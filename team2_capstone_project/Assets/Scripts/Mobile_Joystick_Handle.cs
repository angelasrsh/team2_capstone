using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

/// <summary>
/// A custom floating joystick that anchors at touch position and
/// sends stick values to the Input System without stutter.
/// </summary>
public class Mobile_Joystick_Handle : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform background; // Full joystick background
    [SerializeField] private RectTransform handle;     // Movable handle

    [Header("Settings")]
    [SerializeField] private float movementRange = 100f;  // Max distance in px
    [SerializeField] private bool hideWhenReleased = true;

    [Header("Input System")]
    [SerializeField] private string controlPath = "<Gamepad>/leftStick";

    private Vector2 startPos;
    private Vector2 inputVector = Vector2.zero;

    // Required override for OnScreenControl
    protected override string controlPathInternal
    {
        get => controlPath;
        set => controlPath = value;
    }

    private void Start()
    {
        if (background != null && hideWhenReleased)
            background.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        // Place joystick where touch began
        background.position = eventData.position;
        startPos = eventData.position;

        if (hideWhenReleased)
            background.gameObject.SetActive(true);

        handle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        Vector2 offset = eventData.position - startPos;
        Vector2 clamped = Vector2.ClampMagnitude(offset, movementRange);

        // Move handle
        handle.anchoredPosition = clamped;

        // Normalize [-1..1]
        inputVector = clamped / movementRange;

        // Send to Input System
        SendValueToControl(inputVector);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        SendValueToControl(Vector2.zero);

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        if (background != null && hideWhenReleased)
            background.gameObject.SetActive(false);
    }
}
