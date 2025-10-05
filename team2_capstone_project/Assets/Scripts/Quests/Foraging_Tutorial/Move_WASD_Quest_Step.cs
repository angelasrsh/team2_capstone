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
    [Header("Child canvas object")]
    public Tutorial_Canvas TutorialCanvas;
    [Header("Quest step fields")]
    [TextArea(3, 10)]
    public string InstructionText = "Testing";
    public int InstructionWaitTime = 5;
    public int PopupWaitTime = 10;

    void OnEnable()
    {
        Game_Events_Manager.Instance.onPlayerMove += PlayerMoved;
        TutorialCanvas.DisableAll();

        if (TutorialCanvas == null)
            Debug.Log("[WASD_Q] Error: Tutorial Canvas is null. Please assign it in the prefab inspector");
        else
        {
            TutorialCanvas.DisplayTextDelayed(InstructionText, InstructionWaitTime);
            TutorialCanvas.DisplayGraphicDelayed(PopupWaitTime);
        }
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





}
