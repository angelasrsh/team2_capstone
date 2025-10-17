using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Grimoire;

public class Dialogue_Manager : MonoBehaviour
{
    [Header("Components (Add here)")]
    public TextAsset dialogFile;
    public Dialog_UI_Manager uiManager;
    public System.Action onDialogComplete;
    public Queue<string> dialogQueue = new Queue<string>();

    [Header("Auto-Advancing Dialog Settings")]
    public bool autoAdvanceDialog = false; // Enable auto mode
    public bool autoEndDialogue = false; // Enable auto on dialogue close on end
    public float autoAdvanceDelay = 2.5f;  // Time between lines

    [Header("Character Data")]
    public Dictionary<CustomerData.NPCs, CustomerData> customerDataDict 
        = new Dictionary<CustomerData.NPCs, CustomerData>();
    [SerializeField] private List<CustomerData> customerDataList;
    private Dictionary<string, CustomerData.NPCs> nameToEnumMap = new Dictionary<string, CustomerData.NPCs>
        (StringComparer.OrdinalIgnoreCase);  

    // Internal references
    private Dictionary<string, string> dialogMap;
    private HashSet<string> completedDialogKeys = new HashSet<string>();
    private string myDialogKey;
    [HideInInspector] public enum DialogueState { Normal, Waiting }
    [HideInInspector] public DialogueState currentState = DialogueState.Normal;
    private InputAction interactAction;

    // Components    
    private Player_Controller playerOverworld;

    private void Awake()
    {
        playerOverworld = FindObjectOfType<Player_Controller>();
        dialogMap = new Dictionary<string, string>();

        Player_Input_Controller pic = FindObjectOfType<Player_Input_Controller>();
        if (pic != null)
        {
            interactAction = pic.GetComponent<PlayerInput>().actions["Interact"];
        }

        foreach (var customer in customerDataList)
        {
            if (!customerDataDict.ContainsKey(customer.npcID))
            {
                customerDataDict.Add(customer.npcID, customer);
            }

            // Map name to enum for easy lookup
            if (!string.IsNullOrEmpty(customer.customerName) && !nameToEnumMap.ContainsKey(customer.customerName))
            {
                nameToEnumMap.Add(customer.customerName, customer.npcID);
            }

            PopulateDialogMap();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && dialogQueue.Count == 0 && !uiManager.textTyping)
        {
            EndDialog();
        }
    }

    #region Dialog Map Population
    private bool TryResolveCharacterKey(string key, out CustomerData.NPCs result)
    {
        // 1) direct enum parse (expects names like "Elf" or "Phrog")
        if (Enum.TryParse(key, true, out result))
            return true;

        // 2) try replace spaces with underscores (handles "Asper Agis" -> "Asper_Agis")
        string alt = key.Replace(" ", "_");
        if (Enum.TryParse(alt, true, out result))
            return true;

        // 3) try removing spaces (handles "Asper Agis" -> "AsperAgis")
        string altNoSpace = key.Replace(" ", "");
        if (Enum.TryParse(altNoSpace, true, out result))
            return true;

        // 4) try the display-name map (maps "Asper Agis" -> npcID)
        if (nameToEnumMap.TryGetValue(key, out result))
            return true;

        // 5) fallback: case-insensitive match through the map keys
        foreach (var kv in nameToEnumMap)
        {
            if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                result = kv.Value;
                return true;
            }
        }

        result = default;
        return false;
    }

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
    public void PlayScene(string aDialogKey, bool disablePlayerInput = true)
    {
        Game_Events_Manager.Instance.BeginDialogueBox(aDialogKey);

        if (completedDialogKeys.Contains(aDialogKey) || dialogQueue.Count > 0) 
        {
            PlayNextDialog(disablePlayerInput);
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
        PlayNextDialog(disablePlayerInput);
    }

    /// <summary>
    /// Plays the next line of dialog, parsing emotion from the text itself.
    /// </summary>
    public void PlayNextDialog(bool disablePlayerInput = true)
    {
        if (uiManager.textTyping || currentState == DialogueState.Waiting) return;

        if (dialogQueue.Count > 0)
        {
            uiManager.ClearText();
            string dialogLine = dialogQueue.Dequeue();

            // Parse dialog line -> text {Emotion}
            string[] parts = dialogLine.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            string dialogText = parts[0].Trim(); 
            CustomerData.EmotionPortrait.Emotion emotion = CustomerData.EmotionPortrait.Emotion.Neutral;

            if (parts.Length > 1 && Enum.TryParse(parts[1].Trim(), true, out CustomerData.EmotionPortrait.Emotion parsedEmotion))
            {
                emotion = parsedEmotion;
            }

            // Character from key
            string characterKey = myDialogKey.Split('.')[0];
            if (TryResolveCharacterKey(characterKey, out CustomerData.NPCs charEnum))
            {
                if (customerDataDict.TryGetValue(charEnum, out CustomerData customer))
                {
                    Sprite portrait = customer.defaultPortrait;

                    if (customer.GetEmotionPortraits().TryGetValue(emotion, out Sprite emotionPortrait))
                    {
                        portrait = emotionPortrait;
                    }

                    uiManager.ShowOrHidePortrait(portrait);

                    uiManager.SetDialogSound(customer.dialogClipSound);
                    uiManager.SetDialogSoundSettings(customer.pitchMin, customer.pitchMax);
                    uiManager.SetTypingSpeed(customer.textSpeed);
                }
                else
                {
                    Debug.LogWarning($"Dialogue_Manager: Could not resolve character key '{characterKey}' from dialog key '{myDialogKey}'.");
                }
            }

            uiManager.ShowText(dialogText, disablePlayerInput);

            if (autoAdvanceDialog)
            {
                StartCoroutine(AutoAdvanceNextLine(disablePlayerInput));
            }
        }
        else
        {
            if (autoEndDialogue)
                EndDialog();

            Game_Events_Manager.Instance.DialogueComplete(myDialogKey);
        }
    }
    #endregion

    #region Play Scene (Forced Emotion)
    /// <summary>
    /// Starts a dialog sequence from the given key, but forces the portrait to a given emotion.
    /// Useful for reactions to liked/disliked/neutral dishes.
    /// </summary>
    public void PlayScene(string aDialogKey, CustomerData.EmotionPortrait.Emotion forcedEmotion, bool disablePlayerInput = true)
    {
        // Tell Game events manager so we don't overlap the dialogue box
        Game_Events_Manager.Instance.BeginDialogueBox(aDialogKey);

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
        PlayNextDialog(forcedEmotion, disablePlayerInput);
    }


    /// <summary>
    /// Plays the next line of dialog, forcing a specific emotion (ignores {Braces} in text).
    /// </summary>
    public void PlayNextDialog(CustomerData.EmotionPortrait.Emotion forcedEmotion, bool disablePlayerInput = true)
    {
        if (uiManager.textTyping || currentState == DialogueState.Waiting) return;

        if (dialogQueue.Count > 0)
        {
            uiManager.ClearText();
            string dialogLine = dialogQueue.Dequeue();

            // Ignore braces; use forced emotion
            string[] parts = dialogLine.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            string dialogText = parts[0].Trim();

            var emotion = forcedEmotion;

            // Character from key
            string characterKey = myDialogKey.Split('.')[0];
            if (Enum.TryParse(characterKey, true, out CustomerData.NPCs charEnum))
            {
                if (customerDataDict.TryGetValue(charEnum, out CustomerData customer))
                {
                    Sprite portrait = customer.defaultPortrait;
                    Debug.Log($"Forcing portrait emotion to {emotion} for character {characterKey}");

                    if (customer.GetEmotionPortraits().TryGetValue(emotion, out Sprite emotionPortrait))
                    {
                        portrait = emotionPortrait;
                        Debug.Log($"Found portrait for emotion {emotion}.");
                    }

                    uiManager.ShowOrHidePortrait(portrait);

                    uiManager.SetDialogSound(customer.dialogClipSound);
                    uiManager.SetDialogSoundSettings(customer.pitchMin, customer.pitchMax);
                    uiManager.SetTypingSpeed(customer.textSpeed);
                }
            }

            uiManager.ShowText(dialogText, disablePlayerInput);

            if (autoAdvanceDialog)
            {
                StartCoroutine(AutoAdvanceNextLine(disablePlayerInput));
            }
        }
        else
        {
            if (autoEndDialogue)
                EndDialog();

            Game_Events_Manager.Instance.DialogueComplete(myDialogKey);
        }
    }
    #endregion

    #region Helpers
    public IEnumerator AutoAdvanceNextLine(bool disablePlayerInput = true)
    {
        yield return new WaitUntil(() => uiManager.textTyping == false);
        yield return new WaitForSeconds(autoAdvanceDelay);
        PlayNextDialog(disablePlayerInput);
    }

    public void EndDialog()
    {
        Debug.Log("EndDialog called.");

        uiManager.HideTextBox();
        uiManager.ClearText();
        uiManager.HidePortrait();
        ResetDialogForKey(myDialogKey);
        Player_Input_Controller.instance.EnablePlayerInput();

        onDialogComplete?.Invoke();
        Game_Events_Manager.Instance.EndDialogBox(myDialogKey); // Could probably merge with above
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
