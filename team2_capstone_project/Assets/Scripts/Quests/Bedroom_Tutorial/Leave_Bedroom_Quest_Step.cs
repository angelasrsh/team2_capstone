using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Leave_Bedroom_Quest_Step : Tutorial_Quest_Step
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += LeaveBedroom;


    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// End quest when player exits to a non-main-menu scene
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void LeaveBedroom(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main_Menu")
            FinishQuestStep();

    }
}
