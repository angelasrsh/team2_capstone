using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Journal_Quest_Step : Quest_Step
{

    [Header("Child canvas object")]
    public Tutorial_Canvas TutorialCanvas;
    [Header("Quest step fields")]
    [TextArea(3, 10)]
    public string InstructionText = "Testing";
    public int InstructionWaitTime = 5;
    public int PopupWaitTime = 10;
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
        if (isOpen)
            FinishQuestStep(); // Finish and destroy this object
    }
}
