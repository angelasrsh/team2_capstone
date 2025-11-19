using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Journal_Quest_Step : Dialogue_Quest_Step
{
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onJournalToggle += JournalToggled;
    }
    
    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onJournalToggle -= JournalToggled;
    }

    private void Start()
    {
        DelayedDialogue(0, 0, false);

    }

    private void JournalToggled(bool isOpen)
    {
        if (isOpen)
            FinishQuestStep(); // Finish and destroy this object
    }
}
