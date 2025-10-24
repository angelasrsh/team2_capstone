using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Need to update this
/// </summary>
public class Frying_Instruction_Quest_Step : Dialogue_Quest_Step
{
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onResourceRemove += Cook;
    }
    
    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onResourceRemove -= Cook;
    }
    
    void Start()
    {
        DelayedDialogue(0, 7, false);
    }

    void Cook(Ingredient_Data ing)
    {
        FinishQuestStep();

    }
}
