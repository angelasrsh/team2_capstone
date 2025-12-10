
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Make_Money_Quest_Step : Dialogue_Quest_Step
{

    protected override void OnEnable() => base.OnEnable();
    protected override void OnDisable() => base.OnDisable();

    private void Start()
    {

        QuestStepComplete = true; // Finish and destroy this object
        DelayedDialogue(0, 0, false);
    }
}
