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
    [TextArea(3, 10)]
    public string InstructionText = "Testing";
    public int InstructionWaitTime = 0;
    public int PopupWaitTime = 10;

    private Dialogue_Manager dm;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="delayStart">  Seconds to wait before displaying; default 0 </param>
    /// <param name="delayEnd"> Seconds to wait before hiding. Text will remain on screen until user action if delayHide is 0 or unused </param>
    protected void DelayedDialogue(string key, float delayStart = 0, float delayEnd = 0, bool disablePlayerInput = true)
    {
        // Wait for delayStart, then show text and textbox, then disappear
        StartCoroutine(displayTextDelayed(key, delayStart, delayEnd, disablePlayerInput));

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



