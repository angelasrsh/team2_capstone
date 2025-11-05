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
}
