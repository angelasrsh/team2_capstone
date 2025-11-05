using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Grimoire;
using Ink.Runtime;
using Unity.VisualScripting;

public class Dialogue_Manager : MonoBehaviour
{
    [Header("Components")]
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
    private InputAction talkAction;

    // Components    INK stuff

    private Player_Controller playerOverworld;
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;
    private Story story;


    private bool dialgouePlaying = false;
    private int currentChoiceIndex = -1;

    //new ink dialogue methods

    private void OnEnable()
    {
        Game_Events_Manager.Instance.dialogueEvents.onEnterDialogue += EnterDialogue;
        Game_Events_Manager.Instance.dialogueEvents.onUpdateChoiceIndex += UpdateChoiceIndex;
    }

    private void OnDisable()
    {
        Game_Events_Manager.Instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
        Game_Events_Manager.Instance.dialogueEvents.onUpdateChoiceIndex -= UpdateChoiceIndex;  
    }
    private void UpdateChoiceIndex(int choiceIndex)
    {
        this.currentChoiceIndex = choiceIndex;
    }

    private void SubmitPressed()
    {
        if (!dialgouePlaying)
        {
            return;
        }
        else
        {
            ContinueStory();
        }
    }
    
    private void EnterDialogue(string knotName)
    {
        if (dialgouePlaying)
        {
            return;
        }
        dialgouePlaying = true;

        //inform other parts of our system
        Game_Events_Manager.Instance.dialogueEvents.DialogueStarted();
        if (!knotName.Equals(""))
        {
            story.ChoosePathString(knotName);
            Debug.Log("Entering Dialogue for Knot name:" + knotName);
        }
        else
        {
            Debug.Log("KnotName Not found");
        }
        //kick off the story
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        //make a choice if applciable
        if(story.currentChoices.Count > 0 && currentChoiceIndex != -1)
        {
            story.ChooseChoiceIndex(currentChoiceIndex);
            currentChoiceIndex = -1;
        }
        if (story.canContinue)
        {
            string dialogueLine = story.Continue();
            Debug.Log(dialogueLine);
            Game_Events_Manager.Instance.dialogueEvents.DisplayDialogue(dialogueLine, story.currentChoices);
        }
        else if (story.currentChoices.Count == 0)
        {
            // No more lines, exit dialogue
            ExitDialogue();
        }
    }

    private void ContinueStory()
    {
        // When player presses submit, show next line or exit
        DisplayNextLine();
    }

    private void ExitDialogue()
    {
        Debug.Log("Exiting Dialogue from new ver");
        Game_Events_Manager.Instance.dialogueEvents.DialogueFinished();
        dialgouePlaying = false;
        story.ResetState();
    }
    //

    private void Awake()
    {
        playerOverworld = FindObjectOfType<Player_Controller>();
        dialogMap = new Dictionary<string, string>();
        story = new Story(inkJson.text);

        Player_Input_Controller pic = FindObjectOfType<Player_Input_Controller>();
        if (pic != null)
        {
            talkAction = pic.GetComponent<PlayerInput>().actions["Talk"];
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
        if (talkAction == null) return;
        if (dialgouePlaying && talkAction != null && talkAction.WasPerformedThisFrame())
        {
            ContinueStory();
        }
        if (talkAction.triggered)
        {
            // // If still typing, skip to full line
            // if (uiManager.textTyping)
            //     uiManager.SkipCurrentLineInstant();
            // else if (dialogQueue.Count > 0)
            //     PlayNextDialog();
            // else
            //     EndDialog();
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
        //  1. Look for exact key
        if (dialogMap.TryGetValue(aKey, out string value))
            return value;

        // 2. Collect numbered variants
        List<string> variantKeys = new List<string>();

        foreach (var key in dialogMap.Keys)
        {
            // Match keys that begin with the same base
            if (key.StartsWith(aKey, StringComparison.OrdinalIgnoreCase))
            {
                // Include numbered variants only
                if (key.Length > aKey.Length)
                {
                    string suffix = key.Substring(aKey.Length);
                    if (int.TryParse(suffix, out _))
                    {
                        variantKeys.Add(key);
                    }
                }
            }
        }

        // 3. If any numbered variants exist, pick one randomly
        if (variantKeys.Count > 0)
        {
            string randomKey = variantKeys[UnityEngine.Random.Range(0, variantKeys.Count)];
            return dialogMap[randomKey];
        }

        // 4. No match found at all, fallback
        Debug.LogWarning($"[Dialogue_Manager] No dialog found for key '{aKey}'.");
        return aKey;
    }


    /// <summary>
    /// Returns the resolved dialog key for a base key (e.g. "Phrog.BlindingStew").
    /// If an exact key exists it returns that key; otherwise picks a random numbered variant key
    /// (e.g. "Phrog.BlindingStew1") if any exist; otherwise returns the original baseKey.
    /// </summary>
    public string ResolveDialogKey(string baseKey)
    {
        // if exact key exists, return it
        if (dialogMap.ContainsKey(baseKey))
            return baseKey;

        // Find variant keys that start with baseKey (case-insensitive)
        List<string> variantKeys = new List<string>();
        foreach (var key in dialogMap.Keys)
        {
            if (key.StartsWith(baseKey, StringComparison.OrdinalIgnoreCase))
            {
               // Don't include baseKey itself
                if (!key.Equals(baseKey, StringComparison.OrdinalIgnoreCase))
                    variantKeys.Add(key);
            }
        }

        if (variantKeys.Count > 0)
        {
            return variantKeys[UnityEngine.Random.Range(0, variantKeys.Count)];
        }
        return baseKey;  // Fallback 
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
        StartCoroutine(PlayNextDialogWithDelay(disablePlayerInput));
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

            // Safely replace {player} placeholder with actual player name (fallback = "Chef") and set emotion if present
            dialogText = ProcessDialogueVariables(dialogText);
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
        //INk NEW DIalogue events
        
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
        StartCoroutine(PlayNextForcedEmotionDialogWithDelay(forcedEmotion, disablePlayerInput));
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

            // Replace {player} with player name and set emotion
            dialogText = ProcessDialogueVariables(dialogText);
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

    private string ProcessDialogueVariables(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string playerName = Player_Progress.Instance != null
            ? Player_Progress.Instance.GetPlayerName()
            : "Chef";

        // Replace the variable
        string processed = text.Replace("[player]", playerName);

        // Remove any leftover braces
        processed = processed.Replace("[]", "").Replace("]", "");
        return processed;
    }


    private IEnumerator PlayNextDialogWithDelay(bool disablePlayerInput = true)
    {
        yield return null; // wait one frame for Player_Progress.Instance to initialize
        PlayNextDialog(disablePlayerInput);
    }

    private IEnumerator PlayNextForcedEmotionDialogWithDelay(CustomerData.EmotionPortrait.Emotion forcedEmotion, bool disablePlayerInput = true)
    {
        yield return null; // wait one frame for Player_Progress.Instance to initialize
        PlayNextDialog(forcedEmotion, disablePlayerInput);
    }
    #endregion
}
