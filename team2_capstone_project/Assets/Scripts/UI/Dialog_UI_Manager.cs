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

    [Header("Text Blip Sound Randomization Range")]
    public float lowRandomRange = 0.9f;
    public float highRandomRange = 1.1f;
    public float textTypingSpeed = 0.025f; 

    [Header("Character Portrait")]
    public Image characterPortraitImage;
    [HideInInspector] public bool textTyping = false;
    private bool htmlText = false;

    private Dialogue_Manager dialogManager;
    private Player_Controller playerOverworld; 
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
    }
    
    public void HideTextBox()
    {
        // textBoxAnimation.Close();
        textBoxCanavasGroup.alpha = 0;
        ClearText();
        playerOverworld.EnablePlayerController();
    }

    public void ClearText()
    {
        textBoxText.text = "";
    }
    
    public void ShowText(string aText)
    {
        playerOverworld.DisablePlayerController();
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

                Audio_Manager.instance?.PlaySFX(Audio_Manager.instance?.textSound, 1f,
                    Random.Range(lowRandomRange, highRandomRange));
                yield return new WaitForSeconds(textTypingSpeed); // Typing delay/speed
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
}