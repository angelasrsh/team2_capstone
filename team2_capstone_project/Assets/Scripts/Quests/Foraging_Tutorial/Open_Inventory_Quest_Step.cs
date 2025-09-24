using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Press_I_Quest_Step : Quest_Step
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
