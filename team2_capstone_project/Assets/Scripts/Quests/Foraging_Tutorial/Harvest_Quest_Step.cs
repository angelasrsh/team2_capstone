using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvest_Quest_Step : Tutorial_Quest_Step
{
   

    void OnEnable()
    {
        Game_Events_Manager.Instance.onResourceAdd += Harvest;

        DelayedInstructionStart();
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onResourceAdd -= Harvest;
    }


    private void Harvest(Ingredient_Data ing)
    {
        
        FinishQuestStep(); // Finish and destroy this object 
            
    }
}
