using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Quest_Step : Quest_Step
{
    [Header("Child canvas object")]
    private Tutorial_Canvas TutorialCanvas;

    /* Extra fields for displaying hints or instruction after some time */
    [Header("Quest step fields")]
    [TextArea(3, 10)]
    public string InstructionText = "Testing";
    public int InstructionWaitTime = 0;
    public int PopupWaitTime = 10;

    /// <summary>
    /// Call this to reset the tutorial canvas and begin a delay to display the instruction and image
    /// </summary>
    protected void DelayedInstructionStart()
    {

        TutorialCanvas = GetComponentInChildren<Tutorial_Canvas>();

        TutorialCanvas.DisableAll();

        if (TutorialCanvas == null)
            Debug.Log($"{GetType().Name} on {name} error: could not find a Tutorial_Canvas in children. Please ensure there is a Tutorial_Canvas prefab child gameObject.");
        else
        {
            TutorialCanvas.DisplayTextDelayed(InstructionText, InstructionWaitTime);
            TutorialCanvas.DisplayGraphicDelayed(PopupWaitTime);
        }
    }


}



