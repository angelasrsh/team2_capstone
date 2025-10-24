using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Go_To_Cauldron_Quest_Step : Dialogue_Quest_Step
{
    protected override void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
    }

    protected override void OnDisable()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Cooking_Minigame")
            FinishQuestStep();
    }
}
