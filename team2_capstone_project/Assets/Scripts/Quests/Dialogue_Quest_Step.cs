using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Quest step with functions to play dialogue
/// </summary>
public class Dialogue_Quest_Step : Quest_Step
{

    /* Extra fields for displaying hints or instruction after some time */
    [Header("Quest step fields")]
    public string textKey;
    public string textKeyPC;
    public string postStepTextKey;
    public int InstructionWaitTime = 0;
    public int PopupWaitTime = 10;

    public bool QuestStepComplete { get; set; } = false;

    protected bool dialogueComplete { get; set; } = false;


    private Dialogue_Manager dm;

    virtual protected void OnEnable()
    {
        Game_Events_Manager.Instance.onEndDialogBox += setDialogComplete;
        // Game_Events_Manager.Instance.onEndDialogBox += endQuest;
    }

    virtual protected void OnDisable()
    {
        Game_Events_Manager.Instance.onEndDialogBox -= setDialogComplete;
        // Game_Events_Manager.Instance.onEndDialogBox += endQuest;
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

        if (dialogueComplete && QuestStepComplete)
            FinishQuestStep();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="delayStart">  Seconds to wait before displaying; default 0 </param>
    /// <param name="delayEnd"> Seconds to wait before hiding. Text will remain on screen until user action if delayHide is 0 or unused </param>
    protected void DelayedDialogue(float delayStart = 0, float delayEnd = 0, bool disablePlayerInput = true, string text = "")
    {
        // Order or preference for text: any given text, or mobile/PC based on platform if not empty, or any non-empty text field (textKey or textKeyPC)

        // Given alternative text
        if (text != "")
        {
            StartCoroutine(displayTextDelayed(text, delayStart, delayEnd, disablePlayerInput));
            return;
        }

        // Use default textKey (mobile) or textKeyPC
        if (textKey == "" && textKeyPC == "")
            Helpers.printLabeled(this, "Please assign a dialog.txt textKey in the inspector!");
        else if ((textKey == "") || (SystemInfo.deviceType != DeviceType.Handheld && !simulateMobile && (textKeyPC != "")))
            textKey = textKeyPC;
        // Wait for delayStart, then show text and textbox, then disappear
        StartCoroutine(displayTextDelayed(textKey, delayStart, delayEnd, disablePlayerInput));

    }

    

    
    /// <summary>
    /// Change this quest step's canvas text after delayStart time. Hide after delayHide time.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="delayStart"></param>
    /// <param name="delayHide"> Input 0 to never hide </param>
    /// <returns></returns>
    IEnumerator displayTextDelayed(string key, float delayStart, float delayHide, bool disablePlayerInput = true)
    {
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
        
        if (delayHide > 0)
        {
            yield return new WaitForSeconds(delayHide);
            // Somehow disable text automatically
        }
    }


}



