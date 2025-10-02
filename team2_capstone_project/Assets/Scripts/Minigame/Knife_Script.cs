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

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        knife_is_being_dragged = true;
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
    }


    // Update is called once per frame
    void Update()
    {
        currKnifePosition = transform.position;
    }
}
