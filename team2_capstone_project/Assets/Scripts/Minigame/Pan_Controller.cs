using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Grimoire;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Pan_Controller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  [Header("References")]
  private RectTransform panRedZone; // Pan's red zone RectTransform (used for drop detection)
  [SerializeField] private GameObject errorText; // For any messages to the player
  [SerializeField] private Pan pan;
  private Image panImage; // reference to the pan's Image component
  // private Audio_Manager audio;

  [Header("Settings")]
  private bool isDragging = false;
  private Vector3 panOriginalPos; // used to reset pan position once minigame is done
  private float maxY = 300f;

  [Header("Ingredient Fall Settings")]
  private float spawnInterval = 1f;
  private float fallSpeed = 200f; // pixels per second
  private Ingredient_Data fallingIngredientData;
  private List<Sprite> listOfSprites; // All possible sprites for the falling ingredient
  private List<GameObject> fallingIngredients; // current falling sprites
  private bool isFalling = false;
  private int ingredientsCaught = 0;
  private int ingredientsToCatch; // Number of ingredients to catch before ending fall section

  private void Awake()
  {
    if (pan == null)
      Debug.LogError("[Pan_Controller]: No Pan set in inspector!");

    panImage = GetComponent<Image>();
    if (panImage == null)
      Debug.LogError("[Pan_Controller]: No Image component found on Pan!");

    panRedZone = transform.GetChild(0).GetComponent<RectTransform>();
    if (panRedZone == null)
      Debug.LogError("[Pan_Controller]: No Red Zone found on Pan!");

    fallingIngredients = new List<GameObject>();

    // audio = Audio_Manager.instance;
    // if (SceneManager.GetActiveScene().name == "Frying_Pan_Minigame" && audio == null)
    //   Debug.LogError("[Pan_Controller]: No audio manager instance received from Drag_All!");

    panOriginalPos = transform.position;
  }
  
  /// <summary>
  /// Used to check if another image (falling ingredient) is over the pan's red zone.
  /// </summary>
  private bool IsOverRedZone(RectTransform otherRect)
  {
    if (panRedZone == null || otherRect == null)
        return false;

    // Get world corners of both rects
    Vector3[] zoneCorners = new Vector3[4];
    Vector3[] otherCorners = new Vector3[4];
    panRedZone.GetWorldCorners(zoneCorners);
    otherRect.GetWorldCorners(otherCorners);

    // Convert to Rects (in screen space)
    Rect zoneRect = new Rect(zoneCorners[0], zoneCorners[2] - zoneCorners[0]);
    Rect otherRectScreen = new Rect(otherCorners[0], otherCorners[2] - otherCorners[0]);

    // Return true if they overlap
    return zoneRect.Overlaps(otherRectScreen);
  }

  /// <summary>
  /// Sets the ingredient data for the falling ingredients and prepares the list of sprites.
  /// </summary>
  public void SetFallingIngredient(Ingredient_Data ingredientData)
  {
    if (ingredientData == null)
    {
      Debug.LogError("[Pan_Controller]: No ingredient data provided to SetFallingIngredient!");
      return;
    }

    fallingIngredientData = ingredientData;
    if (fallingIngredientData.CutIngredientImages.Length > 0)
    {
      listOfSprites = new List<Sprite>(fallingIngredientData.CutIngredientImages);
      ingredientsToCatch = listOfSprites.Count; // Set how many to catch based on number of images
    }
    else
    {
      listOfSprites = new List<Sprite>();
      listOfSprites.Add(ingredientData.Image); // Fallback to main image
      ingredientsToCatch = 1;
    }
  }

  /// <summary>
  /// Starts the ingredient fall section of the minigame.
  /// </summary>
  public void StartIngredientFall()
  {
    isFalling = true;
    Debug.Log("Getting here");
    StartCoroutine(SpawnIngredients());
  }

  /// <summary>
  /// Spawns ingredients at intervals until the required number is reached.
  /// </summary>
  private IEnumerator SpawnIngredients()
  {
    while (isFalling && fallingIngredients.Count < ingredientsToCatch)
    {
      SpawnOneIngredient();
      yield return new WaitForSeconds(spawnInterval);
    }
  }

  /// <summary>
  /// Spawns one falling ingredient at a random horizontal position above the screen.
  /// </summary>
  private void SpawnOneIngredient()
  {
    // float randomX = randomX.Range(0f, canvasRect.rect.width);
    float randomX = Random.Range(0f, Screen.width);
    Vector3 spawnPos = new Vector3(randomX, Screen.height + 50f, 0f); // Spawn just above screen

    GameObject ingredientObj = new GameObject("Falling_Ingredient");
    ingredientObj.transform.SetParent(transform.parent); // Set parent to same canvas as pan
    ingredientObj.transform.position = spawnPos;
    // ingredientObj.transform.localScale = Vector3.one * 0.5f; // Scale
    // ingredientObj.AddComponent<CanvasGroup>(); // For proper rendering in canvas
    Image img = ingredientObj.AddComponent<Image>();
    if (listOfSprites.Count > 0)
    {
      int randomIndex = Random.Range(0, listOfSprites.Count);
      img.sprite = listOfSprites[randomIndex];
      listOfSprites.RemoveAt(randomIndex); // Remove to avoid repeats
    }
    else
    {
      Debug.LogError("[Pan_Controller]: No sprites available for falling ingredient!");
      img.sprite = fallingIngredientData.Image; // Fallback to main image
    }
    fallingIngredients.Add(ingredientObj);
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    isDragging = true;
  }

  public void OnDrag(PointerEventData eventData)
  {
    if (!isDragging)
      return;

    // Move pan with pointer, prevent going above maxY or off screen
    Vector3 newPos = eventData.position;
    newPos.y = Mathf.Min(newPos.y, maxY); // Clamp to maxY
    newPos.x = Mathf.Clamp(newPos.x, 0f, Screen.width); // Clamp to screen width
    transform.position = newPos;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    isDragging = false;
  }

  /// <summary>
  /// Handles moving falling ingredients down and checks for catches each frame.
  /// </summary>
  private void LateUpdate()
  {
    if (!isFalling && (fallingIngredients == null || fallingIngredients.Count == 0))
      return;

    // Move all falling ingredients down
    for (int i = fallingIngredients.Count - 1; i >= 0; i--)
    {
      GameObject obj = fallingIngredients[i];
      if (obj == null)
      {
        fallingIngredients.RemoveAt(i);
        continue;
      }

      obj.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

      // Check if over red zone
      RectTransform objRect = obj.GetComponent<RectTransform>();
      if (IsOverRedZone(objRect))
      {
        ingredientsCaught++;
        Destroy(obj);
        fallingIngredients.RemoveAt(i);
        // audio.PlaySound("Catch_Ingredient");

        if (ingredientsCaught >= ingredientsToCatch)
        {
          isFalling = false;
          pan.Invoke(nameof(pan.StartSecondSlider), 1f);
        }
      }
      else if (obj.transform.position.y < -50f) // Off bottom of screen
      {
        Destroy(obj);
        fallingIngredients.RemoveAt(i);
      }
    }
  }

  private void HideErrorText() => errorText.SetActive(false);
}
