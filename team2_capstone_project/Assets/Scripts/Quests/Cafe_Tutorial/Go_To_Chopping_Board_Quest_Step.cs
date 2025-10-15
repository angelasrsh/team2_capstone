using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Go_To_Chopping_Board_Quest_Step : Dialogue_Quest_Step
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

    void Start()
    {
        DelayedDialogue(0, 0);
    }


    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Chopping_Minigame")
            FinishQuestStep();
            
    }
}
