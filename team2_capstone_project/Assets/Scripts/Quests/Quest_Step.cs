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
public abstract class Quest_Step : MonoBehaviour
{
    private bool isFinished = false;
    private bool isPaused = false;
    private string questId;

    private int stepIndex;

    protected bool simulateMobile = true;

    // #if UNITY_EDITOR
        //     simulateMobile = true; // comment this back in with the #if and #endif if you want to simulate mobile in editor
    // #endif


    public void InitializeQuestStep(string questId, int stepIndex)
    {
        this.questId = questId;
        this.stepIndex = stepIndex;
        Game_Events_Manager.Instance.QuestStepChange(questId, stepIndex);

        // Subscribe to pausing quest steps
        Game_Events_Manager.Instance.onSetQuestPaused += SetQuestPaused; // maybe move these to OnEnable or OnDisable?
    }

    /// <summary>
    /// Gets called when a quest is finished to clean up
    /// </summary>
    protected void FinishQuestStep()
    {
        if (isPaused)
            Helpers.printLabeled(this, "Quest step is paused and cannot finish");
        if (!isFinished && !isPaused)
        {
            isFinished = true;
            Game_Events_Manager.Instance.AdvanceQuest(questId);

            // Unsubscribe to pausing quest steps
            Game_Events_Manager.Instance.onSetQuestPaused -= SetQuestPaused;

            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Prevent quest from running when we're in the wrong room
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isPaused"></param>
    protected void SetQuestPaused(string id, bool isPaused)
    {
        if (id == questId)
            this.isPaused = isPaused;

    }

    // protected void ChangeState()
    // {
    //     Game_Events_Manager.Instance.QuestStepChange(questId, stepIndex)
    // }
}
