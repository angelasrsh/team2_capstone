using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Quest step with functions to play dialogue. Please call base.onEnable and base.onDisable!
/// And call DelayedDialogue to say something.
/// </summary>
public class Dialogue_Quest_Step : Quest_Step
{

    /* Extra fields for displaying hints or instruction after some time */
    [Header("Quest step fields")]
    public string textKey;
    public string textKeyPC;
    public string repeatTextKey;
    public string repeatTextKeyPC;
    public string postStepTextKey;
    public int InstructionWaitTime = 0;
    public int RepeatPromptDelay = 10; // Not currently used(?)

    public bool QuestStepComplete { get; set; } = false;

    protected bool dialogueComplete { get; set; } = false;
    
    // private state variables
    // private bool postStepTextKeyDialogStarted = false;


    [HideInInspector] public Dialogue_Manager dm;

    virtual protected void OnEnable()
    {
        Game_Events_Manager.Instance.onDialogueComplete += setDialogComplete;
        // Game_Events_Manager.Instance.onEndDialogBox += endQuest;
    }

    virtual protected void OnDisable()
    {
        Game_Events_Manager.Instance.onDialogueComplete -= setDialogComplete;
        // Game_Events_Manager.Instance.onEndDialogBox += endQuest;
    }

    protected void StartRepeatPrompt()
    {
        string key = repeatTextKey; // Not used- delete?
        if (!(repeatTextKey == "" && repeatTextKeyPC == ""))
            StartCoroutine(RepeatPrompt(key));
            
    }

    /// <summary>
    /// Note whether all this quest's dialogue has been finished or not. 
    /// Add functionality to end quest in child classes if desired
    /// </summary>
    /// <param name="dialogKey"></param>
    virtual protected void setDialogComplete(string dialogKey)
    {
        if (dialogKey == postStepTextKey)
            dialogueComplete = true;
        else if (postStepTextKey == "" && (dialogKey == textKey || dialogKey == textKeyPC))
            dialogueComplete = true;
        // else if (postStepTextKey != "" && !postStepTextKeyDialogStarted)
        // {
        //     DelayedDialogue(0, 0, false, postStepTextKey);
        //     postStepTextKeyDialogStarted = true;
        // }
       
        if (dialogueComplete && QuestStepComplete)
            FinishQuestStep();

        string key = repeatTextKey; // Not used- delete?
        if (!(repeatTextKey != "" || repeatTextKeyPC != ""))
        {
            if (repeatTextKey == "" && repeatTextKeyPC == "")
                Helpers.printLabeled(this, "Please assign a dialog.txt textKey in the inspector!");
            else if ((repeatTextKey == "") || (SystemInfo.deviceType != DeviceType.Handheld && !simulateMobile && (textKeyPC != "")))
                key = repeatTextKeyPC;
            // Wait for delayStart, then show text and textbox, then disappear
            StartCoroutine(displayTextDelayed(key, RepeatPromptDelay));
            
        }
    }
    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="delayStart">  Seconds to wait before displaying; default 0; doesn't do anything right now </param>
    /// <param name="delayEnd"> Seconds to wait before hiding. Text will remain on screen until user action if delayHide is 0 or unused </param>
    protected void DelayedDialogue(float delayStart = 0, float delayEnd = 0, bool disablePlayerInput = false, string text = "")
    {
        // Order or preference for text: any given text, or mobile/PC based on platform if not empty, or any non-empty text field (textKey or textKeyPC)

        // Given alternative text
        if (text != "")
        {
            StartCoroutine(displayTextDelayed(text, delayStart, disablePlayerInput));
            return;
        }

        // Use default textKey (mobile) or textKeyPC
        if (textKey == "" && textKeyPC == "")
            Helpers.printLabeled(this, "Please assign a dialog.txt textKey in the inspector!");
        else if ((textKey == "") || (SystemInfo.deviceType != DeviceType.Handheld && !simulateMobile && (textKeyPC != "")))
            textKey = textKeyPC;
        // Wait for delayStart, then show text and textbox, then disappear
        StartCoroutine(displayTextDelayed(textKey, delayStart, disablePlayerInput));

    }

    IEnumerator RepeatPrompt(string dialogKey) 
    {
        string promptKey;
        if ((repeatTextKey == "") || (SystemInfo.deviceType != DeviceType.Handheld && !simulateMobile && (repeatTextKeyPC != "")))
            promptKey = repeatTextKeyPC;
        else
            promptKey = repeatTextKey;

        if (dm == null)
            dm = FindObjectOfType<Dialogue_Manager>();

        yield return new WaitForSeconds(RepeatPromptDelay);

        if (dm != null)
        {
            dm.PlayScene(promptKey);
        }
        else
        {
            Helpers.printLabeled(this, "Dialogue manager is null");
        }

        StartCoroutine(RepeatPrompt(dialogKey));
        
    }

    

    
    /// <summary>
    /// Change this quest step's canvas text after delayStart time. Hide after delayHide time.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="delayStart"></param>
    /// <param name="delayHide"> Input 0 to never hide </param>
    /// <returns></returns>
    IEnumerator displayTextDelayed(string key, float delayStart, bool disablePlayerInput = false)
    {
        UnityEngine.Debug.Log($"[D_Q_S] displayTextDelayed called for {key} by {GetType()}");

        if (dm == null)
            dm = FindObjectOfType<Dialogue_Manager>();

        yield return new WaitForSeconds(delayStart);

        if (dm != null)
        {
            dm.PlayScene(key, disablePlayerInput);
        }
        else
        {
            Helpers.printLabeled(this, "Dialogue manager is null");
        }
        
    }

    protected void WaitFinishQuestStep(int secondsToWait)
    {
        StartCoroutine(waitFinishQuestStep(secondsToWait));
    }
    IEnumerator waitFinishQuestStep(int secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);
           // Manually close any open dialogue box

        dm = FindObjectOfType<Dialogue_Manager>();

        if (dm != null)
        {
            UnityEngine.Debug.Log("[Tutorial] Manually ending current dialogue.");
            dm.EndDialog();
        }
        FinishQuestStep();
    }


}



