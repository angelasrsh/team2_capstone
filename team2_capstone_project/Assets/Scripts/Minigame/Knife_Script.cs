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
    public float dist = 0f;
    public float seconds = 0f;
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        knife_is_being_dragged = true;

        if (chop_script.lineRenderer == null)
        {
            Debug.LogWarning("LineRend from chop null");
        }

        CheckDist();

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

    void CheckDist()
    {
        if (chop_script.lineRenderer != null)
        {
            Vector3 rectWorld = Camera.main.ScreenToWorldPoint(new Vector3(kPoint.x, kPoint.y, -Camera.main.transform.position.z)); //used to be mouseWorld
            Vector2 p = new Vector2(rectWorld.x, rectWorld.y);

            // Debug.Log($"Mouse p: {p}");
            // Debug.Log($"Point a: {chop_script.lineRenderer.points[0]}");
            // Debug.Log($"Point b: {chop_script.lineRenderer.points[1]}");

            //screen coordinates
            
            dist = chop_script.DistancePointToSegment(p, Camera.main.ScreenToWorldPoint(chop_script.lineRenderer.points[0]),
                Camera.main.ScreenToWorldPoint(chop_script.lineRenderer.points[1])); //compare knife rect position to line1;
            // dist = Mathf.FloorToInt(dist);
            Debug.Log("Distance:" + dist);

            if (dist <= 2f) //if knife is on the line
            {
                Debug.Log("Went close to cut line, starting timer");
                //TODO start timeer
                currentTime += Time.deltaTime;
                seconds = currentTime % 60f;
                Debug.Log("seconds " + seconds);
                // DisplayTime(currentTime);
            }
        }
        else
        {
            Debug.Log("chop_script.lineRenderer.points = null");
        }

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
}
