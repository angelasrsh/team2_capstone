using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Journal_Recipe_Quest_Step : Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onRecipeClick += RecipeClick;
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onRecipeClick -= RecipeClick;
    }


    private void RecipeClick()
    {
        FinishQuestStep(); // Finish and destroy this object
    }
}
