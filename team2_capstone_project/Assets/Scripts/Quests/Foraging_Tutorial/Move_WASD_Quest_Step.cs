using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tutorial Quest Step to make the player move using WASD or arrow key binds
/// </summary>
public class Move_WASD_Quest_Step : QuestStep
{

    void OnEnable()
    {
        Game_Events_Manager.Instance.onPlayerMove += PlayerMoved;
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
