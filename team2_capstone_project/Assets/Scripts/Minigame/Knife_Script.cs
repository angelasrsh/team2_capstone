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

    public Sprite originalSprite;  // Drag the original sprite here in inspector
    [SerializeField] Sprite draggingSprite;      // Drag the dragging sprite here in inspector

    private Vector3 knifeOrigPos;
    public RectTransform knifeRectTransform;
    private Transform parentAfterDrag; //original parent of the drag

    private UnityEngine.UI.Image knifeImage;

    public Vector3 currKnifePosition;

    public bool knife_is_being_dragged = false;

    public event Action OnDragStart;
    public event Action OnDragEnd;

    public Chop_Controller chop_script;
    public void OnBeginDrag(PointerEventData eventData)
    {
        knifeOrigPos = knifeRectTransform.position;
        knifeOrigPos = knifeRectTransform.anchoredPosition; // Use anchoredPosition for UI elements
                                                            // Swap to drag sprite
        if (draggingSprite != null && knifeImage != null)
        {
            knifeImage.sprite = draggingSprite;
        }
        parentAfterDrag = transform.parent;
        knife_is_being_dragged = true;


        OnDragStart?.Invoke();
        Debug.Log("[Knife_Script] OndragStart Invoked");

    }
    public float currentTime = 0f;
    private float dist = 0f;
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        knife_is_being_dragged = true;

        if (chop_script.lineRenderer == null)
        {
            Debug.LogWarning("LineRend from chop null");
        }
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        Vector2 p = new Vector2(mouseWorld.x, mouseWorld.y); 

        // Debug.Log($"Mouse p: {p}");
        // Debug.Log($"Point a: {chop_script.lineRenderer.points[0]}");
        // Debug.Log($"Point b: {chop_script.lineRenderer.points[1]}");

        //screen coordinates
        dist = chop_script.DistancePointToSegment(p, Camera.main.ScreenToWorldPoint(chop_script.lineRenderer.points[0]),
        Camera.main.ScreenToWorldPoint(chop_script.lineRenderer.points[1])); //compare knife rect position to line1;
        Debug.Log("Distance:" + dist);
        if (dist <= 30f) //if knife is on the line
        {
            Debug.Log("Went close to cut line, starting timer");
            //TODO start timeer
            currentTime += Time.deltaTime;
            // DisplayTime(currentTime);
        }



    }

    public void OnEndDrag(PointerEventData eventData)
    {
        knifeRectTransform.anchoredPosition = knifeOrigPos;
        transform.SetParent(parentAfterDrag);
        // Swap back to original sprite
        if (originalSprite != null && knifeImage != null)
        {
            knifeImage.sprite = originalSprite;
        }
        knife_is_being_dragged = false;
        OnDragEnd?.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        knifeRectTransform = GetComponent<RectTransform>();
        parentAfterDrag = transform.parent;
        knifeOrigPos = knifeRectTransform.anchoredPosition;
        knifeImage = GetComponent<UnityEngine.UI.Image>();
        GameObject chop_script_obj = GameObject.Find("ChopController");
        chop_script = chop_script_obj.GetComponent<Chop_Controller>();

        if (chop_script == null)
        {
            Debug.LogWarning("chop is null!");
        }
    }


    // Update is called once per frame
    void Update()
    {
        currKnifePosition = transform.position;
    }
}
