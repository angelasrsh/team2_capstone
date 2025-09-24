using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Stir_Controller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  [Header("References")]
  [SerializeField] private RectTransform cauldronRedZone; // Red zone RectTransform
  [SerializeField] private Animator backgroundAnimator;   // Background animator
  [SerializeField] private Image backgroundImage;         // Background image
  [SerializeField] private Sprite emptyCauldron;          // Normal background
  [SerializeField] private GameObject errorText;          // Cannot stir without ingredients in cauldron

  [Header("Settings")]
  [SerializeField] private float stirDuration = 5f;
  [SerializeField] private float pointerMoveThreshold = 0.1f;

  private Cauldron cauldron;
  private bool isDragging = false;
  private bool isStirring = false;
  private float accumulatedStirTime = 0f;
  private Vector3 lastPointerPos;
  private Vector3 ladleOriginalPos;

  private Image ladleImage; // reference to the ladle’s Image component

  private void Awake()
  {
    cauldron = FindObjectOfType<Cauldron>();
    if (cauldron == null)
      Debug.LogError("No Cauldron found!");
    ladleOriginalPos = transform.position;

    ladleImage = GetComponent<Image>();
    if (ladleImage == null)
      Debug.LogError("No Image component found on Ladle!");
  }

  private void Update()
  {
    if (!isStirring || !isDragging)
      return;

    // Only accumulate stir time if pointer is moving
      Vector3 pointerDelta = Input.mousePosition - lastPointerPos;
    if (pointerDelta.magnitude > pointerMoveThreshold)
    {
      accumulatedStirTime += Time.deltaTime;

      if (backgroundAnimator != null)
        backgroundAnimator.speed = 1f; // resume animation
    }
    else
    {
      if (backgroundAnimator != null)
        backgroundAnimator.speed = 0f; // freeze animation mid-frame
    }

    if (accumulatedStirTime >= stirDuration)
      FinishStirring();

    lastPointerPos = Input.mousePosition;
  }

  private bool IsOverRedZone(Vector2 pointerPos)
  {
    if (cauldronRedZone == null)
      return false;

    Vector3[] corners = new Vector3[4];
    cauldronRedZone.GetWorldCorners(corners);
    Rect rect = new Rect(corners[0], corners[2] - corners[0]);

    return rect.Contains(pointerPos);
  }

  public void OnBeginDrag(PointerEventData eventData)
  {     
    if (cauldron.IsEmpty())
    {
      errorText.SetActive(true);
      errorText.GetComponent<TMP_Text>().text = "Must add at least one ingredient into cauldron before stirring!";
      Invoke(nameof(HideErrorText), 3);
      return;
    }

    isDragging = true;
    lastPointerPos = eventData.position;
  }

  public void OnDrag(PointerEventData eventData)
  {
    if (!isDragging)
      return;

    transform.position = eventData.position;

    if (!isStirring && IsOverRedZone(eventData.position))
      StartStirring();
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    isDragging = false;
    isStirring = false; // shouldn't stir if not dragging ladle
    transform.position = ladleOriginalPos;
    if (ladleImage != null)
      ladleImage.enabled = true;
  }

  private void HideErrorText() => errorText.SetActive(false);
  
  private void StartStirring()
  {
    Debug.Log("Stirring started! Entered red zone.");
    isStirring = true;

    if (ladleImage != null)
      ladleImage.enabled = false;

    if (backgroundAnimator != null)
      backgroundAnimator.SetBool("isStirring", true);

    if (cauldron != null)
      cauldron.StartStirring();
  }

  private void FinishStirring()
  {
    isStirring = false;
    accumulatedStirTime = 0f;
    isDragging = false;

    transform.position = ladleOriginalPos;
    if (ladleImage != null)
      ladleImage.enabled = true;

    if (cauldron != null)
      cauldron.FinishedStir();

    if (backgroundAnimator != null)
    {
      backgroundAnimator.SetBool("isStirring", false);
      Debug.Log("is stirring should be false!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
      // backgroundAnimator.SetBool("isEmpty", true);
    }

    Drag_All.ResetWaterStatus();
  }
}
