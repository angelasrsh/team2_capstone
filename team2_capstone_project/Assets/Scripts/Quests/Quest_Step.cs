using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Credit to https://www.youtube.com/watch?v=UyTJLDGcT64

/// <summary>
/// Extend this class to make unique quest steps.
/// 
/// Derived quest steps get attached to a GameObject to be placed into a scene
/// They also go into the Quest class for their particular quest
/// </summary>
public abstract class QuestStep : MonoBehaviour
{
    private bool isFinished = false;

    /// <summary>
    /// Gets called when a quest is finished to clean up
    /// </summary>
    protected void FinishQuestStep()
    {
        if (!isFinished)
        {
            isFinished = true;
            Destroy(this.gameObject);
        }
    }
}
