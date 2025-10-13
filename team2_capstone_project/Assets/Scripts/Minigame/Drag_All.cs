using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Grimoire;
using TMPro;
using System.Linq;

public class Drag_All : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  // private ICustomDrag onDrag;
  private Transform parentAfterDrag; //original parent of the drag
  private Vector3 ingrOriginalPos;

  [Header("Target Transform")]
  private RectTransform rectTransform;

  public RectTransform redZone;
  public RectTransform redZoneForKnife;


  private Transform resizeCanvas; // Canvas to become child of AND centers itself and scales larger to this canvas

  private Vector3 targetScale = new Vector3(5f, 5f, 5f);

  [Header("Cooking Minigame")]
  private bool isOnPot = false;
  private static Cauldron cauldron; // Static reference to Cauldron script in scene
  private static bool waterAdded = false;
  private static Animator backgroundAnimator;
  private static GameObject errorText;

  [Header("Chopping Minigame")]
  private Canvas canvas;
  public bool canDrag = true;
  private Image currentImage;
  public GameObject newImagePrefab; // Complete prefab to replace with when item is placed on the cutting board for the first time
  public Chop_Controller chopScript;
  public static bool cuttingBoardActive = false; // So that only one ingredient can be on cutting board at a time

  [Header("Inventory Slot Info")]
  public Inventory_Slot ParentSlot; // Since the parent is the UI Canvas otherwise
  [SerializeField] IngredientType ingredientType; // Set in code by parent Inventory_Slot

  [Header("Audio")]
  private static Audio_Manager audio;
  private static bool audioTriggered = false;

  // Start is called before the first frame update
  void Start()
  {

    if (cauldron == null)
      cauldron = FindObjectOfType<Cauldron>();
    ParentSlot = GetComponentInParent<Inventory_Slot>();

    // Find and set animator from animation background
    if (backgroundAnimator == null && SceneManager.GetActiveScene().name == "Cooking_Minigame")
    {
      Transform backgroundCanvas = GameObject.Find("BackgroundCanvas").transform;
      Transform regularBGTransform = backgroundCanvas.Find("Stirring_Animation");
      backgroundAnimator = regularBGTransform.GetComponent<Animator>();
    }

    // Find errorText from Ladle_Canvas
    if (errorText == null && SceneManager.GetActiveScene().name == "Cooking_Minigame")
    {
      Transform ladleCanvas = GameObject.Find("Ladle_Canvas").transform;
      errorText = ladleCanvas.Find("Error_Text").gameObject;
    }

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
      // Debug.Log("- " + comp.GetType().Name);
    }

    if (audio == null)
      audio = Audio_Manager.instance;
    // Reducing restaurant music only for cauldron for now
    if (SceneManager.GetActiveScene().name == "Cooking_Minigame" && !audioTriggered)
    {
      // audio.LowerRestaurantMusic();
      audio.StartFire();
      audioTriggered = true;
    }


    if (SceneManager.GetActiveScene().name == "Chopping_Minigame")
    {
      // Find the ChopController script in the scene
      chopScript = FindObjectOfType<Chop_Controller>();

      if (chopScript == null)
      {
        Debug.LogError("Chop_Controller not found in scene!");
      }
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
  public void OnBeginDrag(PointerEventData eventData)
  {
    // Debug.Log("started drag");
    if (SceneManager.GetActiveScene().name == "Cooking_Minigame" && cauldron.IsStirring())
    {
      errorText.SetActive(true);
      errorText.GetComponent<TMP_Text>().text = "Cannot add more ingredients once you have started stirring!";
      Invoke(nameof(HideErrorText), 3);
      return;
    }

    if (canDrag)
    {
      parentAfterDrag = transform.parent;
      transform.SetParent(transform.root);
      transform.SetAsLastSibling();
      ingrOriginalPos = rectTransform.position;
    }
  }

  public void OnDrag(PointerEventData eventData)
  {
    if (!canDrag || (SceneManager.GetActiveScene().name == "Cooking_Minigame" && cauldron.IsStirring()))
    {
      return;
    }
    else
    {
      transform.position = Input.mousePosition;
    }
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    if (!canDrag || (SceneManager.GetActiveScene().name == "Cooking_Minigame" && cauldron.IsStirring()))
      return;
    else
    {
      // Debug.Log("ended drag");
      transform.SetParent(parentAfterDrag);
      if (IsOverlapping(rectTransform, redZone))
      {
        if (SceneManager.GetActiveScene().name == "Cooking_Minigame")
        {
          // Debug.Log("In RED");
          DuplicateInventorySlot();
          if (!isOnPot)
          {
            cauldron.AddToPot((Ingredient_Data)(ParentSlot.stk.resource));
            if (ParentSlot.stk.resource.Name == "Bone Broth") // Maybe change this later to use enum so that we don't compare strings, but I can't be bothered rn
              BrothAdded();
            Ingredient_Inventory.Instance.RemoveResources(ingredientType, 1);
            isOnPot = true;
          }
        }
        else if (SceneManager.GetActiveScene().name == "Chopping_Minigame")
        {
          Ingredient_Data ingredient_data_var;
          //check if theres nothing on the cutting board, the ingredient instance is actually an ingredient,
          //and that there is a cut ingredient image for that ingredient
          if ((!cuttingBoardActive) && (Ingredient_Inventory.Instance.IngrEnumToData(ingredientType) != null)
                && (((Ingredient_Data)(ParentSlot.stk.resource)).CutIngredientImages.Count() > 0))
          {
            //be able to remove the ingredient from its spot and decrease the count
            ingredient_data_var = Ingredient_Inventory.Instance.IngrEnumToData(ingredientType);
            DuplicateInventorySlot(); //show the ingredient picture after you take it off the inventory slot
            Ingredient_Inventory.Instance.RemoveResources(ingredientType, 1); //decrease the number 
            
            if (resizeCanvas != null)
            {
              // Debug.Log("[drag_all] ingredient type is: " + ingredient_data_var);
              //put the cut image prefab in:
              //hide the image
              transform.gameObject.SetActive(false);
              Debug.Log("ingredient is hidden!");
              canDrag = false;
              //set the ingredient data in chopScript
              chopScript.SetIngredientData(ingredient_data_var, this.gameObject);
              //spawn the cut prefab
              chopScript.ShowIngredientPiecedTogether();
            
            }
            cuttingBoardActive = true;
          }
          else
          {
            rectTransform.position = ingrOriginalPos;
            Debug.Log("[Drag_All]: There is already an ingredient on the cutting board.");
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

  }

  /// <summary>
  /// This function should reset all minigame statuses (including all variables and lists used).
  /// It is in Drag_All, but is currently called in Room_Change_Trigger for the back button in the minigames.
  /// </summary>
  public static void ResetMinigame()
  {
    if (SceneManager.GetActiveScene().name == "Chopping_Minigame")
    {
      // chopScript.ResetChoppingBoard();
      cuttingBoardActive = false;
    }
    else if (SceneManager.GetActiveScene().name == "Cooking_Minigame")
    {
      ResetWaterStatus();
      // cauldron.ResetAll();
      cauldron = null;
      // backgroundAnimator = null;
      // errorText = null;
      // audio = null;
      audioTriggered = false;
    }
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

  /// <summary>
  /// For another object to set this image_slot's ingredient type (used in inventory UI)
  /// </summary>
  /// <param name="iData"></param>
  public void SetIngredientType(Ingredient_Data iData) => ingredientType = Ingredient_Inventory.Instance.IngrDataToEnum(iData);

  public void SetCuttingBoardInactive() => cuttingBoardActive = false;

  /// <summary>
  /// Adds water to pot as ingredient_data and changes cauldron background to fill with water.
  /// Should be called only by the onClick action of the add water button.
  /// </summary>
  public static void AddWaterToPot()
  {
    if (waterAdded)
    {
      errorText.GetComponent<TMP_Text>().text = "Water already added. Cannot add more!";
      errorText.SetActive(true);
      return;
    }

    Ingredient_Data water = Ingredient_Inventory.Instance.getWaterData();
    Debug.Log("[Drag_All]: Added water to cauldron");
    cauldron.AddToPot(water);

    if (backgroundAnimator != null)
    {
      backgroundAnimator.SetBool("hasWater", true);
      backgroundAnimator.SetBool("empty", false);
    }
    waterAdded = true;
    audio.PlayBubblingOnLoop();
  }

  /// <summary>
  /// Once recipe is made, should call this method to reset water status and change
  /// background image to empty cauldron. This method does NOT update the cauldron's list of ingredients.
  /// It only handles resetting the water background animation.
  /// </summary>
  public static void ResetWaterStatus()
  {
    if (backgroundAnimator != null)
    {
      backgroundAnimator.SetBool("hasWater", false);
      // setting broth to false just in case it wasn't correctly set before
      backgroundAnimator.SetBool("hasBroth", false);
      backgroundAnimator.SetBool("empty", true);
    }
    waterAdded = false;
  }

  /// <summary>
  /// Allows cauldron methods to be called in other scripts as long as they can get a Drag_All object;
  /// </summary>
  public Cauldron getCauldron()
  {
    return cauldron;
  }

  /// <summary>
  /// Changes cauldron's not stirring animation to broth colored liquid cauldron version
  /// </summary>
  public static void BrothAdded()
  {
    if (backgroundAnimator != null)
    {
      backgroundAnimator.SetBool("hasBroth", true);
      // setting water to false just in case it wasn't correctly set before
      backgroundAnimator.SetBool("hasWater", false);
      backgroundAnimator.SetBool("empty", false);
    }
    audio.PlayBubblingOnLoop();
  }

  private void HideErrorText() => errorText.SetActive(false);
}
