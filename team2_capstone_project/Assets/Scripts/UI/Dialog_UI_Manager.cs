using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Grimoire;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Dialog_UI_Manager : MonoBehaviour
{
    [Header("Text Box")]
    public TMP_Text textBoxText;
    public CanvasGroup textBoxCanavasGroup;

    [Header("Default Text Blip Sound Settings")]
    public float lowRandomRange = 0.9f;
    public float highRandomRange = 1.1f;
    public float textTypingSpeed = 0.025f;
    public float fastForwardMultiplier = 0.2f;  // smaller = faster typing
    private float currentLowRange;
    private float currentHighRange;
    private float currentTypingSpeed;

    [Header("Next Arrow Indicator")]
    [SerializeField] private Image nextArrow;
    [SerializeField] private float arrowMoveDistance = 10f; // pixels left-right
    [SerializeField] private float arrowMoveSpeed = 2f;
    private Coroutine arrowRoutine;

    [Header("Character Portrait")]
    public Image characterPortraitImage;
    [HideInInspector] public bool textTyping = false;
    private bool htmlText = false;

    [Header("Dialogue Box Animation")]
    public RectTransform textBoxTransform;
    [SerializeField] private float openDuration = 0.5f;
    [SerializeField] private float closeDuration = 0.3f;
    [SerializeField] private float idleAmplitude = 0.01f;
    [SerializeField] private float idleSpeed = 1.5f;

    private Coroutine openRoutine;
    private Coroutine idleRoutine;

    private Dialogue_Manager dialogManager;
    private Player_Controller playerOverworld;
    private AudioClip currentDialogSound;

    // new state vars
    private bool fastForwarding = false;
    private bool skipCurrentLine = false;
    private Coroutine typingCoroutine;

    // input actions
    private InputAction speedAction;
    private InputAction skipAction;

    private void Awake()
    {
        dialogManager = FindObjectOfType<Dialogue_Manager>();
        playerOverworld = FindObjectOfType<Player_Controller>();

        // initial input bind
        TryBindInput();

        // also listen for scene reloads or Game_Manager reinitialization
        SceneManager.sceneLoaded += (scene, mode) => TryBindInput();
    }

    private void OnDestroy()
    {
        try
        {
            UnbindInputs();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Dialog_UI_Manager] Error unbinding inputs on destroy: {ex.Message}");
        }
    }

    private void TryBindInput()
    {
        UnbindInputs();

        PlayerInput playerInput = null;

        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[Dialog_UI_Manager] No PlayerInput found for dialogue controls.");
            return;
        }

        var actions = playerInput.actions;
        if (actions == null)
        {
            Debug.LogWarning("[Dialog_UI_Manager] PlayerInput has no actions asset.");
            return;
        }

        speedAction = actions.FindAction("SpeedDialog", false);
        skipAction = actions.FindAction("SkipDialog", false);

        if (speedAction != null)
        {
            speedAction.performed += OnSpeedPerformed;
            speedAction.canceled += OnSpeedCanceled;
            speedAction.Enable();
        }
        else
        {
            Debug.LogWarning("[Dialog_UI_Manager] SpeedDialog action not found in InputActionAsset.");
        }

        if (skipAction != null)
        {
            skipAction.performed += OnSkipPerformed;
            skipAction.Enable();
        }
        else
            Debug.LogWarning("[Dialog_UI_Manager] SkipDialog action not found in InputActionAsset.");

        // Debug.Log("[Dialog_UI_Manager] Input successfully bound to PlayerInput.");
    }

    private void UnbindInputs()
    {
        if (speedAction != null)
        {
            speedAction.performed -= OnSpeedPerformed;
            speedAction.canceled -= OnSpeedCanceled;
            speedAction.Disable();
            speedAction = null;
        }

        if (skipAction != null)
        {
            skipAction.performed -= OnSkipPerformed;
            skipAction.Disable();
            skipAction = null;
        }
    }

    private void OnSpeedPerformed(InputAction.CallbackContext ctx)
    {
        fastForwarding = true;
        // Debug.Log($"[Dialog_UI_Manager] >>> FastForward ON (this={this.name})");
    }

    private void OnSpeedCanceled(InputAction.CallbackContext ctx)
    {
        fastForwarding = false;
        // Debug.Log($"[Dialog_UI_Manager] >>> FastForward OFF (this={this.name})");
    }

    private void OnSkipPerformed(InputAction.CallbackContext ctx)
    {
        if (this == null || gameObject == null) return;  // destroyed
        if (!isActiveAndEnabled) return;

        if (textTyping)
            skipCurrentLine = true;
        else
        {
            if (dialogManager != null)
                dialogManager.PlayNextDialog();
        }
    }

    private void Start()
    {
        HideTextBox();
        ClearText();

        currentLowRange = lowRandomRange;
        currentHighRange = highRandomRange;
        currentTypingSpeed = textTypingSpeed;
    }

    public void ClearText()
    {
        textBoxText.text = "";
    }

    public void ShowText(string aText)
    {
        HideNextArrow();

        if (openRoutine != null) StopCoroutine(openRoutine);
        openRoutine = StartCoroutine(AnimateOpenBox());

        textBoxCanavasGroup.alpha = 1;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine = StartCoroutine(AddOneCharEnumerator(aText));
    }

    public void HideTextBox()
    {
        if (!this || !gameObject) return;  // destroyed
        if (!isActiveAndEnabled) return;

        if (openRoutine != null)
        {
            try { StopCoroutine(openRoutine); }
            catch { /* do nothing */ }
            openRoutine = null;
        }

        openRoutine = StartCoroutine(AnimateCloseBox());
        ClearText();
    }

    private IEnumerator AddOneCharEnumerator(string aText)
    {
        // Debug.Log($"[Dialog_UI_Manager] Typing started. FastForwarding={fastForwarding}");

        textTyping = true;
        htmlText = false;
        skipCurrentLine = false;
        textBoxText.text = "";

        for (int i = 0; i < aText.Length; i++)
        {
            // Debug.Log($"[Dialog_UI_Manager] Typing char {i}: '{aText[i]}' | Skip={skipCurrentLine} Fast={fastForwarding}");

            if (skipCurrentLine)
            {
                textBoxText.text = aText;
                break;
            }

            if (aText[i] == '<')
                htmlText = true;

            if (!htmlText)
            {
                // Pause command \!
                if (i < aText.Length - 1 && aText[i] == '\\' && aText[i + 1] == '!')
                {
                    i++;
                    yield return new WaitUntil(() =>
                        Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0));
                    continue;
                }

                textBoxText.text += aText[i];
                Audio_Manager.instance?.PlaySFX(currentDialogSound, 1f,
                    Random.Range(currentLowRange, currentHighRange));

                // wait for typing speed duration mid-character
                float elapsed = 0f;
                float target = fastForwarding
                    ? currentTypingSpeed * fastForwardMultiplier
                    : currentTypingSpeed;

                while (elapsed < target && !skipCurrentLine)
                {
                    // if player toggles fastForward mid-wait, recalc instantly
                    target = fastForwarding
                        ? currentTypingSpeed * fastForwardMultiplier
                        : currentTypingSpeed;
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            if (aText[i] == '>')
            {
                htmlText = false;
                int htmlStart = aText.LastIndexOf('<', i);
                if (htmlStart >= 0)
                {
                    string htmlFull = aText.Substring(htmlStart, i - htmlStart + 1);
                    textBoxText.text += htmlFull;
                }
            }
        }

        textTyping = false;
        skipCurrentLine = false;
        typingCoroutine = null;
        fastForwarding = false;
        ShowNextArrow();
    }

    public void SkipCurrentLineInstant()
    {
        if (textTyping && typingCoroutine != null)
            skipCurrentLine = true;
    }

    #region Portraits
    public void ShowOrHidePortrait(Sprite newPortrait)
    {
        if (characterPortraitImage != null && newPortrait != null)
        {
            characterPortraitImage.sprite = newPortrait;
            characterPortraitImage.enabled = true;
        }
        else
        {
            characterPortraitImage.enabled = false;
        }
    }

    public void HidePortrait()
    {
        if (characterPortraitImage == null) return;
        characterPortraitImage.enabled = false;
    }
    #endregion

    #region Dialog Sound Settings
    public void SetDialogSound(AudioClip clip)
    {
        currentDialogSound = clip;
    }

    public void SetDialogSoundSettings(float minPitch, float maxPitch)
    {
        currentLowRange = minPitch;
        currentHighRange = maxPitch;
    }

    public void SetTypingSpeed(float speed)
    {
        currentTypingSpeed = speed;
    }
    #endregion


    #region Animation
    private IEnumerator AnimateOpenBox()
    {
        if (textBoxTransform == null) yield break;

        // Start hidden
        textBoxTransform.localScale = Vector3.zero;
        textBoxCanavasGroup.alpha = 0f;

        float t = 0f;
        while (t < openDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = t / openDuration;

            // Smooth ease-out scaling (grow from center)
            float scale = Mathf.Lerp(0f, 1f, 1 - Mathf.Pow(1 - normalized, 3));
            textBoxTransform.localScale = new Vector3(scale, scale, 1f);
            textBoxCanavasGroup.alpha = normalized;
            yield return null;
        }

        textBoxTransform.localScale = Vector3.one;
        textBoxCanavasGroup.alpha = 1f;

        // Start the idle breathing immediately
        if (idleRoutine != null) StopCoroutine(idleRoutine);
        idleRoutine = StartCoroutine(IdleBreathe());
    }

    private IEnumerator IdleBreathe()
    {
        if (textBoxTransform == null) yield break;

        Vector3 baseScale = Vector3.one;
        float timer = 0f;

        while (true)
        {
            timer += Time.unscaledDeltaTime * idleSpeed;
            float offset = Mathf.Sin(timer) * idleAmplitude;
            textBoxTransform.localScale = baseScale * (1f + offset);
            yield return null;
        }
    }

    private IEnumerator AnimateCloseBox()
    {
        if (idleRoutine != null)
        {
            StopCoroutine(idleRoutine);
            idleRoutine = null;
        }

        Vector3 startScale = textBoxTransform.localScale;
        float startAlpha = textBoxCanavasGroup.alpha;
        float t = 0f;

        while (t < closeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = t / closeDuration;

            float scale = Mathf.Lerp(startScale.x, 0f, Mathf.Pow(normalized, 2));
            textBoxTransform.localScale = new Vector3(scale, scale, 1f);
            textBoxCanavasGroup.alpha = Mathf.Lerp(startAlpha, 0f, normalized);
            yield return null;
        }

        textBoxTransform.localScale = Vector3.zero;
        textBoxCanavasGroup.alpha = 0f;
    }
    #endregion


    #region Next Arrow
    public void ShowNextArrow()
    {
        if (nextArrow == null) return;

        nextArrow.enabled = true;

        if (arrowRoutine != null)
            StopCoroutine(arrowRoutine);

        arrowRoutine = StartCoroutine(AnimateArrow());
    }

    public void HideNextArrow()
    {
        if (nextArrow == null) return;

        nextArrow.enabled = false;

        if (arrowRoutine != null)
        {
            StopCoroutine(arrowRoutine);
            arrowRoutine = null;
        }
    }

    private IEnumerator AnimateArrow()
    {
        RectTransform arrowRect = nextArrow.rectTransform;
        Vector2 startPos = arrowRect.anchoredPosition;
        float timer = 0f;

        while (true)
        {
            timer += Time.unscaledDeltaTime * arrowMoveSpeed;
            float offset = Mathf.Sin(timer) * arrowMoveDistance;
            arrowRect.anchoredPosition = startPos + new Vector2(offset, 0f);
            yield return null;
        }
    }
    #endregion
}
