using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class Mixing_Script : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // private ICustomDrag onDrag;
    Transform parentAfterDrag; //original parent of the drag
    Transform originalPos;
    // int is_spoon_rotated = 0;
    [SerializeField] private RectTransform redZone;
    [SerializeField] private Image redZoneImage; // Reference to the red zone's Image component

    private RectTransform rectTransform;
    [SerializeField] private float requiredDragTime = 3f;
    private bool isDraggingInRedZone = false;
    private float dragTimeInRedZone = 0f;
    private bool hasCompletedDrag = false;
    [SerializeField] private float startingAlpha = 0.3f; // Starting opacity
    [SerializeField] private float maxAlpha = 1f; // Maximum opacity



    public void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log("spoon drag");
        // Reset timing when starting a new drag
        if (!hasCompletedDrag)
        {
            dragTimeInRedZone = 0f;
        }
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        // Check if rotation is already 90 degrees (around Z-axis)
        if (Mathf.Abs(transform.rotation.eulerAngles.z - 90f) > 0.1f) //TODO
        {
            transform.Rotate(0, 0, 90); // Only rotate if not already at 90 degrees
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("dragging spoon");
        // onDrag.OnCurrentDrag();
        transform.position = Input.mousePosition;
        rectTransform.position = transform.position;
        //check if in the red zone
        bool currentlyInRedZone = Drag_All.IsOverlapping(rectTransform, redZone);

        if (currentlyInRedZone && !hasCompletedDrag)
        {
            if (!isDraggingInRedZone) //if you are in the red zone
            {
                isDraggingInRedZone = true;
                Debug.Log("dragging in the red zone, starting time");
            }

            //start the timer
            dragTimeInRedZone += Time.deltaTime;
            // Calculate and set opacity based on drag time
            float progress = dragTimeInRedZone / requiredDragTime;
            float currentAlpha = Mathf.Lerp(startingAlpha, maxAlpha, progress);
            SetRedZoneAlpha(currentAlpha);
            Debug.Log($"Drag time: {dragTimeInRedZone:F1}s / {requiredDragTime}s");

            // Check if we've completed the required time
            if (dragTimeInRedZone >= requiredDragTime)
            {
                CompleteRedZoneDrag();
            }
        }
        else if (isDraggingInRedZone)
        {
            isDraggingInRedZone = false;
            if (!hasCompletedDrag)
            {
                dragTimeInRedZone = 0f;
                Debug.Log("Left red zone - timer reset");
            }
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("ended spoon drag");
        transform.SetParent(parentAfterDrag);
    }

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (redZoneImage != null)
        {
            SetRedZoneAlpha(startingAlpha);
        }
        // onDrag = GetComponent<ICustomDrag>();
        // Debug.Log("Components on " + gameObject.name + ":");
        // foreach(Component comp in GetComponents<Component>())
        // {
        //     Debug.Log("- " + comp.GetType().Name);
        // }

        // onDrag = GetComponent<ICustomDrag>();
        // // Optional: Add a safety check
        // if (onDrag == null)
        // {
        //     Debug.LogError("No ICustomDrag component found on " + gameObject.name);
        // }
    }
    private void SetRedZoneAlpha(float alpha)
    {
        if (redZoneImage != null)
        {
            Color newColor = redZoneImage.color;
            newColor.a = Mathf.Clamp01(alpha); // Ensure alpha is between 0 and 1
            redZoneImage.color = newColor;
        }
    }
    private void CompleteRedZoneDrag()
    {
        if (!hasCompletedDrag)
        {
            hasCompletedDrag = true;
            Debug.Log("Red zone drag completed! Changing to blue.");
            // Change to blue with full opacity
            if (redZoneImage != null)
            {
                redZoneImage.color = new Color(0, 0, 1, maxAlpha); // Blue with full alpha
            }
            
        }
    }
}
