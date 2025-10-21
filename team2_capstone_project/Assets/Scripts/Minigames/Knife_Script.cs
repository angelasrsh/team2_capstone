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
    private Vector3 firstKnifeOrigPos;
    public RectTransform knifeRectTransform;
    private Transform parentAfterDrag; //original parent of the drag
    private Quaternion originalRotation;
    private Quaternion firstOriginalRotation;


    [Header("State")]
    public UnityEngine.UI.Image knifeImage;
    public Vector3 currKnifePosition;
    private bool isSnapped = false;
    private Vector3 snappedPosition;
    private float snappedRotation;
    public bool knife_is_being_dragged = false;

    public event Action OnDragStart;
    public event Action OnDragEnd;
    public event Action OnKnifeSnapped;

    public R_Chop_Controller chop_script;
    
    public float dist = 0f;

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalRotation = transform.rotation;
        knifeOrigPos = knifeRectTransform.anchoredPosition; // Use anchoredPosition for UI elements

        if (draggingSprite != null && knifeImage != null)
        {
            knifeImage.sprite = draggingSprite; // Swap to drag sprite
        }
        parentAfterDrag = transform.parent;
        knife_is_being_dragged = true;
        // isSnapped = false;

        OnDragStart?.Invoke();// shows the cutting lines
        Debug.Log("[Knife_Script] OndragStart Invoked");

    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform CL1R = null;
        if(Drag_All.cuttingBoardActive)
        {
             CL1R = chop_script.GetRedZone().GetComponent<RectTransform>();
        }
        if(CL1R != null)
        {
            Debug.Log($"CLR is {CL1R}");
        }
        // if (isSnapped) //constrain the knife to one axis 
        // {
        //     Debug.Log("Knife is on the line now");
        //     // Get mouse position in screen space
        //     Vector2 mousePos = Input.mousePosition;
        //     // Get the knife's current rotation angle in degrees
        //     float angle = knifeRectTransform.rotation.eulerAngles.z;
        //     // Convert to local space relative to the red zone (rotated coordinate system)
        //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //         CL1R,
        //         mousePos,
        //         null, // Use null for overlay canvas, or your canvas camera
        //         out Vector2 localMousePos
        //     );

        //     // Lock movement to only the local Y axis (vertical in rotated space)
        //     // Keep the X position fixed to the red zone's center
        //     Vector2 constrainedLocalPos = new Vector2(0, localMousePos.y);

        //     // Convert back to world/canvas position
        //     Vector3 worldPos = CL1R.TransformPoint(constrainedLocalPos);

        //     // Apply to knife position
        //     knifeRectTransform.position = worldPos;
        // }
        // else
        {
            transform.position = Input.mousePosition;
        }
        knife_is_being_dragged = true;
        

    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (isSnapped == true) //keep in current position
        {
            Debug.Log("Knife released while snapped!");
            knifeImage.raycastTarget = false;
            OnDragEnd?.Invoke();
            // DON'T reset position or rotation here - knife stays on the line
            return;
        }
        ReturnToOriginalPosition();

        OnDragEnd?.Invoke();
        knife_is_being_dragged = false;

    }

    public void ReturnToOriginalPosition()
    {
        knifeRectTransform.anchoredPosition = firstKnifeOrigPos;
        transform.rotation = firstOriginalRotation;
        transform.SetParent(parentAfterDrag);
        isSnapped = false;
        knifeImage.raycastTarget = true;
        
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
        chop_script = chop_script_obj.GetComponent<R_Chop_Controller>();

        //store position and rotation
        firstOriginalRotation = transform.rotation;
        firstKnifeOrigPos = knifeRectTransform.anchoredPosition; // Use anchoredPosition for UI elements

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
        kPoint = knifeRect.transform.localPosition; // get knife Point position

        currKnifePosition = transform.localPosition;
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
        transform.localRotation = Quaternion.Euler(0, 0, snappedRotation);
        isSnapped = true;
        Debug.Log($"Knife snapped! Position: {snappedPosition}, Rotation: {snappedRotation}");

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
