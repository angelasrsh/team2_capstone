using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Tutorial Quest Step to make the player sprint
/// </summary>
public class Sprint_Quest_Step : Dialogue_Quest_Step
{

    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onPlayerMove += PlayerSprint;
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onPlayerMove -= PlayerSprint;
    }

     void Start()
    {
        DelayedDialogue(10, 0, false);
    }



    private void PlayerSprint()
    {
        FinishQuestStep(); // Finish and destroy this object
    }





}
