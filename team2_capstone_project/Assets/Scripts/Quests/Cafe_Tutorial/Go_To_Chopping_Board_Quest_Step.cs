using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Need to update this
/// </summary>
public class Go_To_Chopping_Board_Quest_Step : Dialogue_Quest_Step
{
    public List<Item_Data> CheckFor; // temp

    protected override void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;
        base.OnDisable();
    }

    void Start()
    {
        DelayedDialogue(0, 0);
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Updated_Restaurant")
        {
            bool hasAll = true;
            foreach (Item_Data ing in CheckFor)
            {
                if (!Ingredient_Inventory.Instance.HasItem(ing))
                    hasAll = false;
            }

            if (hasAll)
                FinishQuestStep();
            else
                DelayedDialogue(0, 0, false, "Journal.Cut_Enough_Items");
        }

    }
}
