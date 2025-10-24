using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combine_Minigame_Quest_Step : Dialogue_Quest_Step
{
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onCombineAddToTable += CombineAddToTable;
        base.OnEnable();   
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onCombineAddToTable -= CombineAddToTable;
        base.OnDisable();
    }

    void Start()
    {
        DelayedDialogue(0, 0, false);
    }

    private void CombineAddToTable(Ingredient_Data ing, int zone)
    {
        FinishQuestStep();
    }   
}
