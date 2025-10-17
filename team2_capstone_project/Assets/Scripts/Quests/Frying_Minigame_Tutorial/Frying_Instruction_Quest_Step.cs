using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Need to update this
/// </summary>
public class Frying_Instruction_Quest_Step : Dialogue_Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onResourceRemove += Cook;


    }
    
    void OnDisable()
    {
        Game_Events_Manager.Instance.onResourceRemove -= Cook;

        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        DelayedDialogue(0, 7, false);
        
    }

    // Update is called once per frame
    void Cook(Ingredient_Data ing)
    {
        FinishQuestStep();

    }
}
