using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Journal_Recipe_Quest_Step : Dialogue_Quest_Step
{
    [SerializeField] private Dish_Data DishToMake; // Which dish the player must click. Set to one-day blinding stew.
    override protected void OnEnable()
    {
        Game_Events_Manager.Instance.onDishDetailsClick += RecipeClick;
        // Game_Events_Manager.Instance.onEndDialogBox += endQuest;

        base.OnEnable();
    }

    // Unsubscribe to clean up
    override protected void OnDisable()
    {
        Game_Events_Manager.Instance.onDishDetailsClick -= RecipeClick;
        // Game_Events_Manager.Instance.onEndDialogBox -= endQuest;

        base.OnDisable();
    }

    private void Start()
    {
        DelayedDialogue(0, 0, false);
    }


    /// <summary>
    /// Note that recipe has been clicked and finish quest step if the journal is done talking
    /// </summary>
    /// <param name="dishData"></param>
    private void RecipeClick(Dish_Data dishData)
    {
        if (dishData == DishToMake)
        {
            QuestStepComplete = true;
        }

        if (QuestStepComplete && dialogueComplete)
            FinishQuestStep();
    }

    override protected void setDialogComplete(string dialogKey)
    {
        base.setDialogComplete(dialogKey); // 
    }

    // private void endQuest()
    // {
    //     FinishQuestStep();
    // }
}
