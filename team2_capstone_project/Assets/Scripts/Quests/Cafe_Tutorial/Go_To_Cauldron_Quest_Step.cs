using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Go_To_Cauldron_Quest_Step : Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onMakeCauldronDish += MakeCauldronDish;
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onMakeCauldronDish -= MakeCauldronDish;
    }


    private void MakeCauldronDish()
    {
        FinishQuestStep(); // Finish and destroy this object
    }
}
