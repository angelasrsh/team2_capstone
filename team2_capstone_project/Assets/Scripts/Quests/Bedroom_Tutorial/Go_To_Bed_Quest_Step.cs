using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Go_To_Bed_Quest_Step : Dialogue_Quest_Step
{
    protected override void OnEnable()
    {
        Day_Turnover_Manager.OnDayEnded += WentToBed;
        SceneManager.sceneLoaded += onSceneLoaded;


        //DelayedInstructionStart();
    }

    protected override void OnDisable()
    {
        Day_Turnover_Manager.OnDayEnded -= WentToBed;
        SceneManager.sceneLoaded -= onSceneLoaded;

    }


    /// <summary>
    /// End quest when player exits to a non-main-menu scene
    /// </summary>
    /// <param name="currentRoom"> RoomID of the room being left </param>
    /// <param name="exitingTo"> RoomID of the room being entered </param>
    void WentToBed(Day_Summary_Data summary)
    {
        FinishQuestStep();

    }

      private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Bedroom" && Day_Turnover_Manager.Instance.currentTimeOfDay == Day_Turnover_Manager.TimeOfDay.Evening)
        {
            DelayedDialogue(0, 0, false);
        }
            
    }
}
