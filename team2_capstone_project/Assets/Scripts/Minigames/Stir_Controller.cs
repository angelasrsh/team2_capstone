using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Grimoire;
using UnityEngine.SceneManagement;

public class Stir_Controller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  [Header("References")]
  [SerializeField] private RectTransform cauldronRedZone; // Red zone RectTransform
  [SerializeField] private Animator backgroundAnimator;   // Background animator
  [SerializeField] private Image backgroundImage;         // Background image
  [SerializeField] private Sprite emptyCauldron;          // Normal background
  [SerializeField] private GameObject errorText;          // Cannot stir without ingredients in cauldron

  [Header("Settings")]
  [SerializeField] private float stirDuration;
  [SerializeField] private float pointerMoveThreshold = 0.1f;

  private Cauldron cauldron;
  private bool isDragging = false;
  private bool isStirring = false;
  private float accumulatedStirTime = 0f;
  private Vector3 lastPointerPos;
  private Vector3 ladleOriginalPos;

  private Image ladleImage; // reference to the ladle’s Image component
  private Audio_Manager audioManager;

  private void Awake()
  {
    cauldron = FindObjectOfType<Cauldron>();
    if (cauldron == null)
      Debug.LogError("[Stir_Controller]: No Cauldron found!");
    ladleOriginalPos = transform.position;

    ladleImage = GetComponent<Image>();
    if (ladleImage == null)
      Debug.LogError("[Stir_Controller]: No Image component found on Ladle!");

    audioManager = Audio_Manager.instance;
    if (SceneManager.GetActiveScene().name == "Cooking_Minigame" && audioManager == null)
      Debug.LogError("[Stir_Controller]: No audio manager instance received from Drag_All!");
  }

  private bool isMoving = false;
  private float idleTime = 0f;
  [SerializeField] private float idleThreshold = 0.2f; // how long the mouse must stop before stopping audio

  private void Update()
  {
    if (!isStirring || !isDragging)
      return;

    Vector3 pointerDelta = Input.mousePosition - lastPointerPos;

    if (pointerDelta.magnitude > pointerMoveThreshold)
    {
        accumulatedStirTime += Time.deltaTime;
        idleTime = 0f;

        if (!isMoving)
        {
            isMoving = true;
            audioManager.PlayStirringOnLoop();

            if (cauldron != null)
            {
                cauldron.SetPlayerStirring(true);
                cauldron.ResumeStirring();
            }
        }

        if (backgroundAnimator != null)
            backgroundAnimator.speed = 1f;
    }

    else
    {
        idleTime += Time.deltaTime;

        if (idleTime > idleThreshold && isMoving)
        {
            isMoving = false;
            audioManager.StopStirring();

            if (cauldron != null)
            {
                cauldron.SetPlayerStirring(false);
                cauldron.PauseStirring(); // pause only
            }

            if (backgroundAnimator != null)
                backgroundAnimator.speed = 0f;
        }
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
      isStirring = false;
      audioManager.StopStirring();

      if (cauldron != null)
      {
          // tell cauldron the player stopped moving
          cauldron.SetPlayerStirring(false);

          // stop the stirring coroutine / preserve paused state
          cauldron.StopStirringCompletely();
      }

      if (backgroundAnimator != null)
          backgroundAnimator.SetBool("isStirring", false);

      transform.position = ladleOriginalPos;
      if (ladleImage != null)
          ladleImage.enabled = true;
  }

  private void HideErrorText() => errorText.SetActive(false);
  
  private void StartStirring()
  {
    Debug.Log("[Stir_Controller]: Stirring started! Entered red zone.");
    isStirring = true;
    audioManager.PlayStirringOnLoop();

    if (ladleImage != null)
      ladleImage.enabled = false;

    if (backgroundAnimator != null)
      backgroundAnimator.SetBool("isStirring", true);

    if (cauldron != null)
      cauldron.StartStirring(stirDuration);
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
      backgroundAnimator.SetBool("isStirring", false);

    Drag_All.ResetWaterStatus();
    audioManager.StopStirring();
    audioManager.StopBubbling();
  }
}
