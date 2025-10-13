using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System; //for actions



public class Knife_Script : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Sprites")]
    public Sprite originalSprite;  // Drag the original sprite here in inspector
    [SerializeField] Sprite draggingSprite;      // Drag the dragging sprite here in inspector

    [Header("Transform References")]
    private Vector3 knifeOrigPos;
    public RectTransform knifeRectTransform;
    private Transform parentAfterDrag; //original parent of the drag
    private Quaternion originalRotation;

    [Header("State")]
    private UnityEngine.UI.Image knifeImage;
    public Vector3 currKnifePosition;
    private bool isSnapped = false;
    private Vector3 snappedPosition;
    private float snappedRotation;
    public bool knife_is_being_dragged = false;

    public event Action OnDragStart;
    public event Action OnDragEnd;
    public event Action OnKnifeSnapped;

    public Chop_Controller chop_script;
    
    public float dist = 0f;

    public void OnBeginDrag(PointerEventData eventData)
    {
        knifeOrigPos = knifeRectTransform.position;
        originalRotation = transform.rotation;
        knifeOrigPos = knifeRectTransform.anchoredPosition; // Use anchoredPosition for UI elements

        if (draggingSprite != null && knifeImage != null)
        {
            knifeImage.sprite = draggingSprite; // Swap to drag sprite
        }
        parentAfterDrag = transform.parent;
        knife_is_being_dragged = true;
        isSnapped = false;

        OnDragStart?.Invoke();// shows the cutting lines
        Debug.Log("[Knife_Script] OndragStart Invoked");

    }

    public void OnDrag(PointerEventData eventData)
    {
        // If knife is already snapped, don't allow movement
        if (isSnapped)
        {
            return;
        }
        // SnapToLine();
        transform.position = Input.mousePosition;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        knife_is_being_dragged = false;

        if (isSnapped == true) //keep in current position
        {
            Debug.Log("Knife released while snapped!");
            OnDragEnd?.Invoke();
            // DON'T reset position or rotation here - knife stays on the line
            return;
        }
        ReturnToOriginalPosition();

        OnDragEnd?.Invoke();
    }

    public void ReturnToOriginalPosition()
    {
        knifeRectTransform.anchoredPosition = knifeOrigPos;
        transform.rotation = originalRotation;
        transform.SetParent(parentAfterDrag);
        isSnapped = false;
        
        // Swap back to original sprite
        if (originalSprite != null && knifeImage != null)
        {
            knifeImage.sprite = originalSprite;
        }
        
        Debug.Log("Knife returned to original position");
    }

    // Start is called before the first frame update
    void Start()
    {
        //fidn the rectTransform of the knife, and the knife point
        knifePoint = GameObject.Find("KnifePoint");
        knifeRect = knifePoint.GetComponent<RectTransform>();


        knifeRectTransform = GetComponent<RectTransform>();//this one is to return knife to the og position
        parentAfterDrag = transform.parent;
        knifeOrigPos = knifeRectTransform.anchoredPosition; //return the knife to this position

        knifeImage = GetComponent<UnityEngine.UI.Image>();
        GameObject chop_script_obj = GameObject.Find("ChopController");
        chop_script = chop_script_obj.GetComponent<Chop_Controller>();

        if (chop_script == null)
        {
            Debug.LogWarning("chop is null!");
        }
    }

    private float CheckDist()
    {
        if (chop_script.lineRenderer != null)
        {
            Debug.Log("Distance:" + dist);
        }
        else
        {
            Debug.Log("chop_script.lineRenderer.points = null");
            dist = -1;
        }
        return dist;
    }
    // Update is called once per frame
    public RectTransform knifeRect;
    private GameObject knifePoint;
    public Vector2 kPoint;

    void Update()
    {
        kPoint = knifeRect.transform.position; // get knife Point position

        currKnifePosition = transform.position;
    }

    public void SnapToLine()
    {
        Debug.Log("Entered SnapToLine");
        if (isSnapped) return; // Already snapped

        if (chop_script == null)
        {
            return;
        }


        // Calculate line rotation
        float lineRotation = chop_script.GetLineRotation();

        // Snap knife to line
        snappedRotation = lineRotation;

        // Apply rotation (align knife with line)
        transform.localRotation = Quaternion.Euler(0, 0, lineRotation);

        isSnapped = true;

        Debug.Log($"Knife snapped! Position: {snappedPosition}, Rotation: {lineRotation}");

        // Invoke snapped event
        OnKnifeSnapped?.Invoke();
    }

    // Public method to check if knife is currently snapped
    public bool IsSnapped()
    {
        return isSnapped;
    }

    // Public method to force unsnap (if needed)
    public void ForceUnsnap()
    {
        isSnapped = false;
    }
}
