using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Drag_All : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  // private ICustomDrag onDrag;
  private Transform parentAfterDrag; //original parent of the drag
  private Vector3 ingrOriginalPos;

  [Header("Target Transform")]
  private RectTransform rectTransform;

  private RectTransform redZone;


  private Transform resizeCanvas; // Canvas to become child of AND centers itself and scales larger to this canvas

  private Vector3 targetScale = new Vector3(5f, 5f, 5f);

  [Header("Cooking Minigame")]
  private bool isOnPot = false;
  private static Cauldron cauldron; // Static reference to Cauldron script in scene
  private static bool waterAdded = false;
  private static Image regularBG;
  private static Sprite originalBGSprite;

  [Header("Chopping Minigame")]
  private Canvas canvas;
  public bool canDrag = true;
  private Image currentImage;
  public GameObject newImagePrefab; // Complete prefab to replace with when item is placed on the cutting board for the first time
  public Chop_Controller chopScript;
  private static bool cuttingBoardActive = false; // So that only one ingredient can be on cutting board at a time

  [Header("Inventory Slot Info")]
  public Inventory_Slot ParentSlot; // Since the parent is the UI Canvas otherwise
  [SerializeField] IngredientType ingredientType; // Set in code by parent Inventory_Slot


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

    parentAfterDrag = transform.parent;
    transform.SetParent(transform.root);
    transform.SetAsLastSibling();
    ingrOriginalPos = rectTransform.position;
  }

  public void OnDrag(PointerEventData eventData)
  {
    transform.position = Input.mousePosition;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    // Debug.Log("ended drag");
    transform.SetParent(parentAfterDrag);
    if (IsOverlapping(rectTransform, redZone))
    {
      // The Inventory UI requires an image slot, so duplicate and replace self
      GameObject newImageSlot = Instantiate(this.gameObject, ParentSlot.transform);
      this.name = "Image_Slot_Old";
      newImageSlot.name = "Image_Slot"; // Must rename so Inventory_Slot can find the new image_slot
      newImageSlot.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
      newImageSlot.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
      if (!cuttingBoardActive)
        Ingredient_Inventory.Instance.RemoveResources(ingredientType, 1);
      if (SceneManager.GetActiveScene().name == "Cooking_Minigame")
      {
        // Debug.Log("In RED");
        if (!isOnPot)
        {
          cauldron.AddToPot((Ingredient_Data)(ParentSlot.stk.resource));
          isOnPot = true;
        }
      }
      else if (SceneManager.GetActiveScene().name == "Chopping_Minigame")
      {
        //TODO: Call function to show the cutting lines + the enlarged ingredient here (bottom code should be in function)
        //make the ingredient from the inventory Bigger:
        if (!cuttingBoardActive)
        {
          if (resizeCanvas != null)
          {
            transform.SetParent(resizeCanvas);
            transform.localPosition = Vector3.zero; // Center within the target canvas
            transform.localScale = targetScale;
          }
          canDrag = false;
          cuttingBoardActive = true;
        }
      }
    }
    else
    {
      if (SceneManager.GetActiveScene().name == "Cooking_Minigame")
      {
        if (isOnPot)
        {
          cauldron.RemoveFromPot((Ingredient_Data)(ParentSlot.stk.resource));
          isOnPot = false;
        }
      }
      // Debug.Log("Not in RED, snapping back");
      rectTransform.position = ingrOriginalPos;
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    if (cauldron == null)
      cauldron = FindObjectOfType<Cauldron>();
    ParentSlot = GetComponentInParent<Inventory_Slot>();

    GameObject red_zone_found = GameObject.Find("RedZone");
    if (red_zone_found != null)
      redZone = red_zone_found.GetComponent<RectTransform>();
    else
    {
      Debug.Log("[Drag_All] Could not find redZone!");
    }
    GameObject resizeCanvas_object = GameObject.Find("IngredientResize-Canvas");
    if (resizeCanvas_object != null)
      resizeCanvas = resizeCanvas_object.GetComponent<RectTransform>();
    else
    {
      Debug.Log("[Drag_All] Could not find Ingredient Resize Canvas!");
    }

    rectTransform = GetComponent<RectTransform>();

    Debug.Log("Components on " + gameObject.name + ":");
    foreach (Component comp in GetComponents<Component>())
    {
      Debug.Log("- " + comp.GetType().Name);
    }
  }

  /// <summary>
  /// For another object to set this image_slot's ingredient type (used in inventory UI)
  /// </summary>
  /// <param name="iData"></param>
  public void SetIngredientType(Ingredient_Data iData)
  {
    ingredientType = Ingredient_Inventory.Instance.IngrDataToEnum(iData);
  }

  public void SetCuttingBoardInactive()
  {
    cuttingBoardActive = false;
  }

  /// <summary>
  /// Adds water to pot as ingredient_data and changes cauldron background to fill with water.
  /// Should be called only by the onClick action of the add water button.
  /// </summary>
  public static void AddWaterToPot()
  {
    if (waterAdded)
      return;

    Ingredient_Data water = Ingredient_Inventory.Instance.getWaterData();
    Debug.Log("[Drag_All]: Added water to cauldron");
    cauldron.AddToPot(water);

    if (regularBG == null)
    {
      // Find Background image and change it to the one with water in cauldron
      Transform backgroundCanvas = GameObject.Find("BackgroundCanvas").transform;
      Transform regularBGTransform = backgroundCanvas.Find("Regular_BG");
      regularBG = regularBGTransform.GetComponent<Image>();
      originalBGSprite = regularBG.sprite;
    }
    regularBG.sprite = water.Image;
    waterAdded = true;
  }

  /// <summary>
  /// Once recipe is made, should call this method to reset water status and change
  /// background image to empty cauldron. TEST THIS OUT LATER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
  /// </summary>
  public static void resetWaterStatus()
  {
    // if (regularBG == null)
    //   Debug.Log("regularBG is null");
    // if (originalBGSprite == null)
    //   Debug.Log("originalBGSprite is null");
    regularBG.sprite = originalBGSprite;
    waterAdded = false;
  }
}
