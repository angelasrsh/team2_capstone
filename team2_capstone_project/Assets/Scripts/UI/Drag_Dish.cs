using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Grimoire;
// using TMPro;
using System.Linq;

public class Drag_Dish :  MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  private Transform parentAfterDrag; //original parent of the drag
  private Vector3 dishOriginalPos;
  private bool canDrag = false;

  [Header("Target Transform")]
  private RectTransform rectTransform;

  [Header("Trash Can")]
  private static Trash trash; // Static reference to Trash script in scene
  private static RectTransform trashRedZone;

  [Header("Inventory Slot Info")]
  public Inventory_Slot ParentSlot; // Since the parent is the UI Canvas otherwise
  [SerializeField] IngredientType ingredientType; // Set in code by parent Inventory_Slot

  [Header("Audio")]
  private static Audio_Manager audioManager;
  // private static bool audioTriggered = false;

  // Start is called before the first frame update
  void Start()
  {  
    ParentSlot = GetComponentInParent<Inventory_Slot>();
    rectTransform = GetComponent<RectTransform>();

    if (audioManager == null)
      audioManager = Audio_Manager.instance;

    if (SceneManager.GetActiveScene().name == "Updated_Restaurant") // CHANGE THIS IF LATER CHANGING NAME OF UPDATED RESTAURANT
    {
      trash ??= FindObjectOfType<Trash>();
      trashRedZone = trash.redZone;
    }
  }
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

  private void OnEnable()
  {
    Trash.OnTrashOpenChanged += SetCanDrag;
    Chest.OnChestOpenChanged += SetCanDrag;

    SceneManager.activeSceneChanged += OnSceneChanged;
  }

  private void OnDisable()
  {
    Trash.OnTrashOpenChanged -= SetCanDrag;
    Chest.OnChestOpenChanged -= SetCanDrag;

    SceneManager.activeSceneChanged -= OnSceneChanged;
  }

  private void OnSceneChanged(Scene previousScene, Scene newScene)
  {
    if (newScene.name == "Updated_Restaurant") // add in world map later if we end up not wanting to drag these in world map too
      canDrag = false;
    else
      canDrag = true;
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    // Debug.Log("started drag");
    if (canDrag)
    {
      parentAfterDrag = transform.parent;
      transform.SetParent(transform.root);
      transform.SetAsLastSibling();
      dishOriginalPos = rectTransform.position;
    }
  }

  public void OnDrag(PointerEventData eventData)
  {
    if (!canDrag)
      return;
    
    transform.position = Input.mousePosition;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    if (!canDrag)
      return;

    if (SceneManager.GetActiveScene().name == "Updated_Restaurant" && IsOverlapping(rectTransform, trashRedZone)) // CHANGE THIS IF LATER CHANGING NAME OF UPDATED RESTAURANT
    {
      if (trash == null)
      {
        trash = FindObjectOfType<Trash>();
        trashRedZone = trash.redZone;
      }

      if (!trash.trashOpen) // just a safety check. Shouldn't need to do this if canDrag is set up properly
      {
        rectTransform.position = dishOriginalPos;
        return;
      }

      DuplicateInventorySlot();
      Dish_Data dish = (Dish_Data)(ParentSlot.stk.resource);
      int trashed = trash.AddItemToTrash(dish, 1);
      if (trashed > 0) // Only remove dish if actually added to trash
      {
        Dish_Tool_Inventory.Instance.RemoveResources(dish, 1);
      }
      else
        rectTransform.position = dishOriginalPos;
      Destroy(gameObject);
      return;
    }

    transform.position = dishOriginalPos;
  }

  /// <summary>
  /// The Inventory UI requires an image slot, so duplicate and replace self
  /// </summary>
  private void DuplicateInventorySlot()
  {
    GameObject newImageSlot = Instantiate(this.gameObject, ParentSlot.transform);
    this.name = "Image_Slot_Old";
    newImageSlot.name = "Image_Slot"; // Must rename so Inventory_Slot can find the new image_slot
    newImageSlot.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
    newImageSlot.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
  }

  public void SetCanDrag(bool draggable)
  {
    canDrag = draggable;
  }
}
