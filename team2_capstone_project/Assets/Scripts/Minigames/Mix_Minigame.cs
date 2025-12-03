using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Grimoire;
using TMPro;

public class Mix_Minigame : MonoBehaviour
{
    public static Mix_Minigame Instance;

    private float duration;
    private float targetCPS;
    private float tolerance;
    private float timer = 0f;
    private int clicks = 0;

    private System.Action onSuccess;
    private System.Action<string> onFail;
    private bool active = false;
    [SerializeField] private TextMeshProUGUI mixText;

    [Header("Shake Animation Info")]
    [SerializeField] private RectTransform shakerPanel;
    private float shakeSpeed = 0f;
    private float shakeAmount = 30f; // How far up/down the shaker moves
    private float shakeDecay = 5f; // How fast shake slows down when not spamming
    // private float baseIdleBobSpeed = 1f; // Slow idle motion
    // private float baseIdleBobAmount = 5f;
    private Vector2 shakerStartPos;

    [Header("Player Input Info")]
    // private Player_Controller player;
    private InputAction interactAction;
    private PlayerInput playerInput;

    private void Awake()
    {
        Instance = this;
        if (mixText != null)
            mixText.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();
        if (playerInput != null)
            interactAction = playerInput.actions["Interact"];

        if (shakerPanel != null)
            shakerStartPos = shakerPanel.anchoredPosition;
    }

    public void StartMinigame(float duration, float targetCPS, float tolerance,
                              System.Action success, System.Action<string> fail)
    {
        this.duration = duration;
        this.targetCPS = targetCPS;
        this.tolerance = tolerance;

        onSuccess = success;
        onFail = fail;

        clicks = 0;
        timer = 0f;
        active = true;

        // Subscribe to interact action
        interactAction.performed += OnInteractPressed;
        interactAction.Enable();

        if (mixText != null)
        {
            mixText.text = "Spam Interact Button!"; // might change to spam E for pc and spam action button in mobile later
            mixText.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!active) 
            return;

        timer += Time.deltaTime;

        if (timer >= duration)
            Finish();
        
        UpdateShakerAnimation();
    }

    /// <summary>
    /// Called when interact button is pressed. Used to count how many times it has been pressed.
    /// </summary>
    /// <param name="ctx"></param>
    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (!active) 
            return;

        clicks++;

        // Increase shake speed based on player input (increase quickly when spamming)
        shakeSpeed += 1.5f;

        // Visual feedback
        if (mixText != null)
            mixText.text = $"Clicks: {clicks}";
    }

    private void UpdateShakerAnimation()
    {
        if (shakerPanel == null)
            return;

        // idle mode
        if (shakeSpeed < 0.1f)
            return;

        // shaking mode (based on spam speed)
        float shakeY = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
        shakerPanel.anchoredPosition = shakerStartPos + new Vector2(0, shakeY);

        // Gradually reduce shakeSpeed over time
        shakeSpeed = Mathf.Lerp(shakeSpeed, 0, Time.deltaTime * shakeDecay);
    }

    private void Finish()
    {
        float cps = clicks / duration;
        
        shakerPanel.anchoredPosition = shakerStartPos;
        shakeSpeed = 0f;

        if (cps < targetCPS - tolerance)
        {
            StartCoroutine(RestartAfterDelay());
            return;
        }

        active = false;
        if (mixText != null)
            mixText.gameObject.SetActive(false);

        if (cps > targetCPS + tolerance)
        {
            onFail?.Invoke("Too fast!");
            return;
        }

        onSuccess?.Invoke();
    }

    private IEnumerator RestartAfterDelay()
    {
        mixText.text = "Too slow! Restarting...";
        yield return new WaitForSeconds(1.5f);

        // Reset minigame values
        clicks = 0;
        timer = 0f;
        shakeSpeed = 0f;

        mixText.text = "Spam Interact Button!";
    }
}
