using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Grimoire;

public class Dialog_UI_Manager : MonoBehaviour
{
    [Header("Text Box")]
    public TMP_Text textBoxText;
    public CanvasGroup textBoxCanavasGroup;

    [Header("Default Text Blip Sound Settings")]
    public float lowRandomRange = 0.9f;
    public float highRandomRange = 1.1f;
    public float textTypingSpeed = 0.025f;

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
    // private TextBoxAnimation textBoxAnimation;

    private void Awake()
    {
        dialogManager = FindObjectOfType<Dialogue_Manager>();
        playerOverworld = FindObjectOfType<Player_Controller>();
        // textBoxAnimation = GetComponentInChildren<TextBoxAnimation>();
    }

    private void Start()
    {
        HideTextBox();
        ClearText();

        // Initialize dialog sound settings w/ default values
        currentLowRange = lowRandomRange;
        currentHighRange = highRandomRange;
        currentTypingSpeed = textTypingSpeed;
    }

    public void HideTextBox()
    {
        // textBoxAnimation.Close();
        textBoxCanavasGroup.alpha = 0;
        ClearText();
        Player_Input_Controller.instance.EnablePlayerInput();
    }

    public void ClearText()
    {
        textBoxText.text = "";
    }

    public void ShowText(string aText)
    {
        Player_Input_Controller.instance.DisablePlayerInput();
        textBoxCanavasGroup.alpha = 1;
        // textBoxAnimation.Open();
        StartCoroutine(AddOneCharEnumerator(aText));
    }

    private IEnumerator AddOneCharEnumerator(string aText)
    {
        textTyping = true;

        for (int i = 0; i < aText.Length; i++)
        {
            // Handle HTML tags
            if (aText[i] == '<')
            {
                htmlText = true;
            }

            if (!htmlText)
            {
                // Detect the "\!" pause command
                if (i < aText.Length - 1 && aText[i] == '\\' && aText[i + 1] == '!')
                {
                    i++; // Skip the "!" character
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0));
                    continue;
                }

                // Add normal characters to the text
                textBoxText.text += aText[i];

                Audio_Manager.instance?.PlaySFX(currentDialogSound, 1f, Random.Range(currentLowRange, currentHighRange));
                Debug.Log($"Playing sound: {currentDialogSound} | AudioManager instance: {Audio_Manager.instance}");
                yield return new WaitForSeconds(currentTypingSpeed);  // Typing delay/speed
            }

            // Handle HTML tags
            if (aText[i] == '>')
            {
                htmlText = false;

                int htmlStart = aText.LastIndexOf('<', i);
                string htmlFull = aText.Substring(htmlStart, i - htmlStart + 1);
                textBoxText.text += htmlFull;  // Add full HTML tag to the text
            }
        }

        textTyping = false;
    }

    #region Portraits
    public void ShowOrHidePortrait(Sprite newPortrait)
    {
        if (characterPortraitImage != null && newPortrait != null)
        {
            characterPortraitImage.sprite = newPortrait;
            characterPortraitImage.enabled = true;
            Debug.Log("Character portrait updated.");
        }
        else
        {
            Debug.LogWarning("Character portrait image or new portrait sprite is null.");
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
        Debug.Log($"UI Manager received clip: {clip}");
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