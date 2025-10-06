using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Tutorial Quest Step to make the player move using WASD or arrow key binds
/// </summary>
public class Move_WASD_Quest_Step : Tutorial_Quest_Step
{

    void OnEnable()
    {
        Game_Events_Manager.Instance.onPlayerMove += PlayerMoved;
        DelayedInstructionStart();
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
