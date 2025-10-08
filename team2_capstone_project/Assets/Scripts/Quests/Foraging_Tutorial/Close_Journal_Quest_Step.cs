using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the player close the journal
/// TODO: Would be nice to track if this happened in the past before the player got here.
/// Maybe put most of these on one quest step? Makes linear dialogue harder, though.
/// </summary>
public class Close_Journal_Quest_Step : Tutorial_Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onJournalToggle += JournalToggled;
        DelayedInstructionStart();
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onJournalToggle -= JournalToggled;
    }


    private void JournalToggled(bool isOpen)
    {
        if (!isOpen)
            FinishQuestStep(); // Finish and destroy this object
    }
}
