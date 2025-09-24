using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class Knife_Script : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    public Sprite originalSprite;  // Drag the original sprite here in inspector
    [SerializeField] Sprite draggingSprite;      // Drag the dragging sprite here in inspector

    private Vector3 knifeOrigPos;
    public RectTransform knifeRectTransform;
    private Transform parentAfterDrag; //original parent of the drag

    private Image knifeImage;

    public Vector3 currKnifePosition; 


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
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
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
    }

    // Start is called before the first frame update
    void Start()
    {
        knifeRectTransform = GetComponent<RectTransform>();
        parentAfterDrag = transform.parent;
        knifeOrigPos = knifeRectTransform.anchoredPosition;
        knifeImage = GetComponent<Image>();
    }


    // Update is called once per frame
    void Update()
    {
        currKnifePosition = transform.position;
    }
}
