using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public interface ICustomDrag
{
  void OnCurrentDrag();
  void EndDrag();
  void startDrag();
}


public class Drag_All : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ICustomDrag onDrag;
    Transform parentAfterDrag; //original parent of the drag
    Transform originalPos;

    [Header("Target Transform")]
    private RectTransform rectTransform;


    [SerializeField] private RectTransform cuttingBoardRect;
    [SerializeField] private Transform targetCanvas; // Canvas to become child of AND center within

    [SerializeField] private Vector3 targetScale = Vector3.one;



    private Canvas canvas;
    private bool canDrag = true;
    private Image currentImage;
    public GameObject newImagePrefab; // Complete prefab to replace with



    public static bool IsOverlapping(RectTransform rectA, RectTransform rectB)
    {
        //checks if the rectangles are overlapping
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
        if (!canDrag) //not supposed to be dragging but you cant
        {
            changePrefab();
            return;
        }
        onDrag.startDrag();
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("dragging");
        if (onDrag != null)
        {
            onDrag.OnCurrentDrag();
        }
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("ended drag");

        if (SceneManager.GetActiveScene().name == "Cooking_Minigame")
        {
            onDrag.EndDrag();
            transform.SetParent(parentAfterDrag);
        }
        else if (SceneManager.GetActiveScene().name == "Chopping_Minigame")
        {
            transform.SetParent(parentAfterDrag);
            if (IsOverlapping(rectTransform, cuttingBoardRect))
            {
                //TODO: Call function to show the cutting lines + the enlarged ingredient here (bottom code should be in function)

                //make the ingredient from the inventory Bigger:
                if (targetCanvas != null)
                {
                    transform.SetParent(targetCanvas);
                    transform.localPosition = Vector3.zero; // Center within the target canvas
                }
                transform.localScale = targetScale;
                canDrag = false;

            }

        }

    }



    // Start is called before the first frame update
    void Start()
    {
        // onDrag = GetComponent<ICustomDrag>();
        rectTransform = GetComponent<RectTransform>();

        Debug.Log("Components on " + gameObject.name + ":");
        foreach (Component comp in GetComponents<Component>())
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
    void Update()
    {
        

    }

    private void changePrefab()
    {
        if (currentImage != null)
        {
            Debug.Log("changing not sliced to sliced image");
            // Store original position and parent
            Transform originalParent = transform.parent;
            Vector3 originalPosition = transform.localPosition;
            Vector3 originalScale = transform.localScale;

            // Create new object
            GameObject newObject = Instantiate(newImagePrefab, originalParent);
            newObject.transform.localPosition = originalPosition;
            newObject.transform.localScale = originalScale;

            // Destroy old object
            Destroy(gameObject);
            return;
        }
    }
}
