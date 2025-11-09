using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Need to update this
/// </summary>
public class Go_To_Chopping_Board_Quest_Step : Dialogue_Quest_Step
{
    public Item_Data checkfor; // temp

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
        if (scene.name == "Updated_Restaurant" && Dish_Tool_Inventory.Instance.HasItem(checkfor))
            FinishQuestStep();

    }
}
