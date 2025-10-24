using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the player close the journal
/// TODO: Would be nice to track if this happened in the past before the player got here.
/// Maybe put most of these on one quest step? Makes linear dialogue harder, though.
/// </summary>
public class Close_Journal_Quest_Step : Dialogue_Quest_Step
{
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onJournalToggle += JournalToggled;
        DelayedDialogue(10, 0, false);
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onJournalToggle -= JournalToggled;
    }

    void Start()
    {
        if (!Journal_Menu.Instance.isPaused) // Immediately end if journal is already closed
            FinishQuestStep(); // Possible bug if player leaves game in-between and then returns
    }

    private void JournalToggled(bool isOpen)
    {
        if (!isOpen)
            FinishQuestStep(); // Finish and destroy this object
    }
}
