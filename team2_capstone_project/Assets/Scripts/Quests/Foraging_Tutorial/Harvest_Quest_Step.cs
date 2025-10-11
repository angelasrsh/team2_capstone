using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvest_Quest_Step : Quest_Step
{
   void OnEnable()
    {
        Game_Events_Manager.Instance.onHarvest += Harvest;
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onHarvest -= Harvest;
    }


    private void Harvest()
    {
        FinishQuestStep(); // Finish and destroy this object
    }
}
