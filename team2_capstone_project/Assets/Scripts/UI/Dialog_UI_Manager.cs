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

    [Header("Character Portrait")]
    public Image characterPortraitImage;
    [HideInInspector] public bool textTyping = false;
    private bool htmlText = false;

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
        {
            Debug.LogWarning("[Dialog_UI_Manager] SkipDialog action not found in InputActionAsset.");
        }

        // Debug.Log("[Dialog_UI_Manager] Input successfully bound to PlayerInput.");
    }

    private void UnbindInputs()
    {
        if (speedAction != null)
        {
            speedAction.performed -= OnSpeedPerformed;
            speedAction.canceled  -= OnSpeedCanceled;
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
        // Debug.Log($"[Dialog_UI_Manager] >>> Skip Pressed (this={this.name}) | textTyping={textTyping}");

        if (textTyping)
        {
            skipCurrentLine = true;
        }
        else
        {
            dialogManager?.PlayNextDialog();
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

    public void HideTextBox()
    {
        if (textBoxCanavasGroup == null) return;

        textBoxCanavasGroup.alpha = 0;
        ClearText();

        // Player_Input_Controller.instance?.EnablePlayerInput();
    }


    public void ClearText()
    {
        textBoxText.text = "";
    }

    public void ShowText(string aText, bool disablePlayerInput = true)
    {
        // if (disablePlayerInput)
        //     Player_Input_Controller.instance?.DisablePlayerInput();

        textBoxCanavasGroup.alpha = 1;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine = StartCoroutine(AddOneCharEnumerator(aText));
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
}
