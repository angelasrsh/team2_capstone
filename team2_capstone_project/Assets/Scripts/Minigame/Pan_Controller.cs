using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Grimoire;
using UnityEngine.SceneManagement;

public class Pan_Controller : MonoBehaviour
{
  [Header("References")]
  private RectTransform panRedZone; // Pan's red zone RectTransform (used for drop detection)
  // [SerializeField] private Animator backgroundAnimator;   // Background animator
  // [SerializeField] private Image backgroundImage;         // Background image
  // [SerializeField] private Sprite emptyCauldron;          // Normal background
  [SerializeField] private GameObject errorText;          // For any messages to the player
  private Pan pan;
  private Image panImage; // reference to the pan's Image component
  // private Audio_Manager audio;

  [Header("Settings")]
  private bool isDragging = false;
  private Vector3 lastPointerPos;
  private Vector3 panOriginalPos; // used to reset pan position once minigame is done


  private void Awake()
  {
    pan = FindObjectOfType<Pan>();
    if (pan == null)
      Debug.LogError("[Pan_Controller]: No Pan found!");

    panImage = GetComponent<Image>();
    if (panImage == null)
      Debug.LogError("[Pan_Controller]: No Image component found on Pan!");

    panRedZone = transform.GetChild(0).GetComponent<RectTransform>();
    if (panRedZone == null)
      Debug.LogError("[Pan_Controller]: No Red Zone found on Pan!");

    // audio = Audio_Manager.instance;
    // if (SceneManager.GetActiveScene().name == "Frying_Pan_Minigame" && audio == null)
    //   Debug.LogError("[Pan_Controller]: No audio manager instance received from Drag_All!");
  }

  private void Update()
  {
    // if (!isDragging)
    //   return;

    // lastPointerPos = Input.mousePosition;
  }
  
  /// <summary>
  /// Used to check if another image (e.g., ingredient) is over the pan's red zone.
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
  
  // private bool IsOverRedZone(Vector2 pointerPos)
  // {
  //   if (panRedZone == null)
  //     return false;

  //   Vector3[] corners = new Vector3[4];
  //   panRedZone.GetWorldCorners(corners);
  //   Rect rect = new Rect(corners[0], corners[2] - corners[0]);

  //   return rect.Contains(pointerPos);
  // }

  public void OnBeginDrag(PointerEventData eventData)
  {     
    isDragging = true;
    // lastPointerPos = eventData.position;
  }

  public void OnDrag(PointerEventData eventData)
  {
    if (!isDragging)
      return;

    transform.position = eventData.position;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    isDragging = false;
  }

  private void HideErrorText() => errorText.SetActive(false);
}
