using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ICustomDrag
{
    void OnCurrentDrag();
}


public class DragAll : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ICustomDrag onDrag;
    Transform parentAfterDrag; //original parent of the drag
    Transform originalPos;

    public static bool IsOverlapping(RectTransform rectA, RectTransform rectB)
    {
        if (rectA == null || rectB == null) //if one of the objects doesnt exist
        {
            return false;
        }
        Vector3[] cornersA = new Vector3[4];
        Vector3[] cornersB = new Vector3[4];

        rectA.GetWorldCorners(cornersA); //fil rectA UI local positions into world postions
        rectB.GetWorldCorners(cornersB);

        Rect rect1 = new Rect(cornersA[0], cornersA[2] - cornersA[0]); //find the width and height in the world corner
        Rect rect2 = new Rect(cornersB[0], cornersB[2] - cornersB[0]);

        return rect1.Overlaps(rect2);

    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log("started drag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("dragging");
        onDrag.OnCurrentDrag();
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("ended drag");
        transform.SetParent(parentAfterDrag);

    }

    // Start is called before the first frame update
    void Start()
    {
        // onDrag = GetComponent<ICustomDrag>();
        Debug.Log("Components on " + gameObject.name + ":");
        foreach(Component comp in GetComponents<Component>())
        {
            Debug.Log("- " + comp.GetType().Name);
        }
        
        onDrag = GetComponent<ICustomDrag>();
        // Optional: Add a safety check
        if (onDrag == null)
        {
            Debug.LogError("No ICustomDrag component found on " + gameObject.name);
        }
    }
}
