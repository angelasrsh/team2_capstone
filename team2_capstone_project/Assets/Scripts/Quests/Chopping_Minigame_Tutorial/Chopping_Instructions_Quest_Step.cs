using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Need to update this
/// </summary>
public class Chopping_Instructions_Quest_Step : Dialogue_Quest_Step
{
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onResourceAdd += ItemCut;
    }
    
    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onResourceAdd -= ItemCut;
    }
    
    void Start()
    {
        DelayedDialogue(0, 0, false);
    }

    void ItemCut(Ingredient_Data ing)
    {
        DelayedDialogue(0, 0, false, "Journal.Chopped_One_Thing");
        WaitFinishQuestStep(5);
    }
}
