using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Leave_Bedroom_Quest_Step : Tutorial_Quest_Step
{
    // Start is called before the first frame update
    void OnEnable()
    {
        SceneManager.sceneLoaded += LeaveBedroom;

        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
        if (dm != null)
        {
            string fillerKey = $"Tutorial.Bedroom";
            dm.PlayScene(fillerKey, CustomerData.EmotionPortrait.Emotion.Neutral);
        }

        DelayedInstructionStart();


    }

    // Update is called once per frame
    void OnDisable()
    {
        SceneManager.sceneLoaded -= LeaveBedroom;

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
