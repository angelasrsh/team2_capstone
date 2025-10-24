using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvest_Quest_Step : Dialogue_Quest_Step
{


    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onResourceAdd += Harvest;
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onResourceAdd -= Harvest;
    }

    void Start()
    {
        if (Ingredient_Inventory.Instance.TotalIngCount > 3)
            FinishQuestStep();
        else
            DelayedDialogue(0, 0, false);
    }

    private void Harvest(Ingredient_Data ing)
    {        
        FinishQuestStep(); // Finish and destroy this object 
            
    }
}
