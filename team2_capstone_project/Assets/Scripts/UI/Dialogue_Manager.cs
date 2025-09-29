using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogue_Manager : MonoBehaviour
{
    [Header("Components (Add here)")]
    public TextAsset dialogFile;
    public Dialog_UI_Manager uiManager;
    public System.Action onDialogComplete;
    public Queue<string> dialogQueue = new Queue<string>();

    [Header("Auto-Advancing Dialog Settings")]
    public bool autoAdvanceDialog = false; // Enable auto mode
    public float autoAdvanceDelay = 2.5f;  // Time between lines

    [Header("Character Portraits")]
    public Dictionary<Character_Portrait_Data.CharacterName, Character_Portrait_Data> characterPortraits 
        = new Dictionary<Character_Portrait_Data.CharacterName, Character_Portrait_Data>();
    [SerializeField] private List<Character_Portrait_Data> characterPortraitList;

    // Internal references
    private Dictionary<string, string> dialogMap;
    private HashSet<string> completedDialogKeys = new HashSet<string>();
    private string myDialogKey;
    [HideInInspector] public enum DialogueState { Normal, Waiting }
    [HideInInspector] public DialogueState currentState = DialogueState.Normal;

    // Components    
    private Player_Controller playerOverworld;

    private void Awake()
    {
        playerOverworld = FindObjectOfType<Player_Controller>();
        
        dialogMap = new Dictionary<string, string>();

        foreach (var characterPortrait in characterPortraitList)
        {
            if (!characterPortraits.ContainsKey(characterPortrait.characterName))
            {
                characterPortraits.Add(characterPortrait.characterName, characterPortrait);
            }
        }
        PopulateDialogMap();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && dialogQueue.Count == 0 && !uiManager.textTyping)
        {
            EndDialog();
        }
    }

    #region Dialog Map Population
    private void PopulateDialogMap()
    {
        if (dialogMap.Count > 0 || dialogFile == null)
            return;

        string[] lines = dialogFile.text.Split(new[] { '\n' }, StringSplitOptions.None);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.Contains("="))
            {
                string[] split = line.Split(new[] { '=' }, 2);
                string key = split[0].Trim();
                string value = split.Length > 1 ? split[1].Trim() : "";

                if (!dialogMap.ContainsKey(key))
                {
                    dialogMap[key] = value;
                }
                else
                {
                    // allow multiple lines per key separated by \n
                    dialogMap[key] += "\n" + value;
                }
            }
        }
        Debug.Log($"Loaded {dialogMap.Count} dialog entries.");
    }

    public string GetDialogFromKey(string aKey)
    {
        if (dialogMap.TryGetValue(aKey, out string value))
        {
            return value;
        }
        return aKey;
    }
    #endregion

    #region Play Scene (Normal)
    /// <summary>
    /// Starts a dialog sequence from the given key.
    /// Parses emotions from {Braces} in lines if present.
    /// </summary>
    public void PlayScene(string aDialogKey)
    {
        if (completedDialogKeys.Contains(aDialogKey) || dialogQueue.Count > 0) 
        {
            PlayNextDialog();
            return;
        }

        myDialogKey = aDialogKey; 
        string dialogText = GetDialogFromKey(aDialogKey);
        string[] lines = dialogText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            dialogQueue.Enqueue(line);
        }
        completedDialogKeys.Add(aDialogKey); 
        Debug.Log($"Queue populated with {dialogQueue.Count} lines for key: {aDialogKey}");
        PlayNextDialog();
    }

    /// <summary>
    /// Plays the next line of dialog, parsing emotion from the text itself.
    /// </summary>
    public void PlayNextDialog()
    {
        if (uiManager.textTyping || currentState == DialogueState.Waiting) return;

        if (dialogQueue.Count > 0)
        {
            string dialogLine = dialogQueue.Dequeue();

            // Parse dialog line -> text {Emotion}
            string[] parts = dialogLine.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            string dialogText = parts[0].Trim(); 
            Character_Portrait_Data.EmotionPortrait.Emotion emotion = Character_Portrait_Data.EmotionPortrait.Emotion.Neutral;

            if (parts.Length > 1 && Enum.TryParse(parts[1].Trim(), true, out Character_Portrait_Data.EmotionPortrait.Emotion parsedEmotion))
            {
                emotion = parsedEmotion;
            }

            // Character from key
            string characterKey = myDialogKey.Split('.')[0];
            if (Enum.TryParse(characterKey, true, out Character_Portrait_Data.CharacterName charEnum))
            {
                if (characterPortraits.TryGetValue(charEnum, out Character_Portrait_Data characterData))
                {
                    Sprite portrait = characterData.defaultPortrait;

                    if (characterData.GetEmotionPortraits().TryGetValue(emotion, out Sprite emotionPortrait))
                    {
                        portrait = emotionPortrait;
                    }

                    uiManager.ShowOrHidePortrait(portrait);
                }
            }

            uiManager.ShowText(dialogText);

            if (autoAdvanceDialog)
            {
                StartCoroutine(AutoAdvanceNextLine());
            }
        }
        else
        {
            EndDialog();
        }
    }
    #endregion

    #region Play Scene (Forced Emotion)
    /// <summary>
    /// Starts a dialog sequence from the given key, but forces the portrait to a given emotion.
    /// Useful for reactions to liked/disliked/neutral dishes.
    /// </summary>
    public void PlayScene(string aDialogKey, Character_Portrait_Data.EmotionPortrait.Emotion forcedEmotion)
    {
        // Tell Game events manager so we don't overlap the dialogue box
        Game_Events_Manager.Instance.BeginDialogueBox();

        if (dialogQueue.Count > 0)
        {
            PlayNextDialog(forcedEmotion);
            return;
        }

        myDialogKey = aDialogKey;
        string dialogText = GetDialogFromKey(aDialogKey);

        string[] lines = dialogText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            dialogQueue.Enqueue(line);
        }

        Debug.Log($"Queue populated with {dialogQueue.Count} lines for key: {aDialogKey}");
        PlayNextDialog(forcedEmotion);
    }


    /// <summary>
    /// Plays the next line of dialog, forcing a specific emotion (ignores {Braces} in text).
    /// </summary>
    public void PlayNextDialog(Character_Portrait_Data.EmotionPortrait.Emotion forcedEmotion)
    {
        if (uiManager.textTyping || currentState == DialogueState.Waiting) return;

        if (dialogQueue.Count > 0)
        {
            string dialogLine = dialogQueue.Dequeue();

            // Ignore braces; use forced emotion
            string[] parts = dialogLine.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            string dialogText = parts[0].Trim();

            var emotion = forcedEmotion;

            // Character from key
            string characterKey = myDialogKey.Split('.')[0];
            if (Enum.TryParse(characterKey, true, out Character_Portrait_Data.CharacterName charEnum))
            {
                if (characterPortraits.TryGetValue(charEnum, out Character_Portrait_Data characterData))
                {
                    Sprite portrait = characterData.defaultPortrait;
                    Debug.Log($"Forcing portrait emotion to {emotion} for character {characterKey}");

                    if (characterData.GetEmotionPortraits().TryGetValue(emotion, out Sprite emotionPortrait))
                    {
                        portrait = emotionPortrait;
                        Debug.Log($"Found portrait for emotion {emotion}.");
                    }

                    uiManager.ShowOrHidePortrait(portrait);
                }
            }

            uiManager.ShowText(dialogText);

            if (autoAdvanceDialog)
            {
                StartCoroutine(AutoAdvanceNextLine());
            }
        }
        else
        {
            EndDialog();
        }
    }
    #endregion

    #region Helpers
    public IEnumerator AutoAdvanceNextLine()
    {
        yield return new WaitUntil(() => uiManager.textTyping == false);
        yield return new WaitForSeconds(autoAdvanceDelay);
        uiManager.ClearText();
        PlayNextDialog();
    }

    public void EndDialog()
    {
        Debug.Log("EndDialog called.");

        uiManager.HideTextBox();
        uiManager.ClearText();
        uiManager.HidePortrait();
        ResetDialogForKey(myDialogKey);
        playerOverworld.EnablePlayerController();

        onDialogComplete?.Invoke();
        Game_Events_Manager.Instance.EndDialogBox(); // Could probably merge with above
    }

    public void ResetDialogForKey(string aDialogKey)
    {
        if (completedDialogKeys.Contains(aDialogKey))
        {
            completedDialogKeys.Remove(aDialogKey);
        }
    }
    #endregion
}
