using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Tutorial Quest Step to make the player move using WASD or arrow key binds
/// </summary>
public class Move_WASD_Quest_Step : Quest_Step
{
    public Tutorial_Canvas TutorialCanvas;
    public String InstructionText = "Testing";
    public int instructionWaitTime;
    public int popupWaitTime;

    void OnEnable()
    {
        Game_Events_Manager.Instance.onPlayerMove += PlayerMoved;
        enableInstruction();
        enableHelpGraphic();
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onPlayerMove -= PlayerMoved;
    }


    private void PlayerMoved()
    {
        FinishQuestStep(); // Finish and destroy this object
    }

    public void enableInstruction()
    {
        TutorialCanvas.DisplayText(InstructionText);
    }

    public void enableHelpGraphic()
    {
        TutorialCanvas.DisplayHighlight();
    }

}
