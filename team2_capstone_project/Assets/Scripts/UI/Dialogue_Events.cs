using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dialogue_Events
{
    //new dialogue using ink
    // [Header("Ink Dialogue")]
    public event Action<string> onEnterDialogue;
    public void EnterDialogue(string knotName) //knot - the name of the narration portion in ink
    {
        if (onEnterDialogue != null)
        {
            onEnterDialogue(knotName);
        }
    }

    public event Action onDialogueStarted;
    public void DialogueStarted()
    {
        if (onDialogueStarted != null)
        {
            onDialogueStarted();
        }
    }
    public event Action onDialogueFinished;

    public void DialogueFinished()
    {
        if (onDialogueFinished != null)
        {
            onDialogueFinished();
        }
    }
    public event Action<string> onDisplayDialogue;
    public void DisplayDialogue(string dialogueLine)
    {
        Debug.Log("Display Dialogue for new NPC stuff");
        if (onDisplayDialogue != null)
        {
            onDisplayDialogue(dialogueLine);
        }
    }

}
