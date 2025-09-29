using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Go_To_Cauldron_Quest_Step : Quest_Step
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Cooking_Minigame")
            FinishQuestStep();
            
    }
}
