using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Make_Bone_Broth_Quest_Step : Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onJournalToggle += JournalToggled;
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
