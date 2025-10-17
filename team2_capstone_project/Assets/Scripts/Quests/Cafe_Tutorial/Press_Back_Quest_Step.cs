using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Same as Enter_Restaurant_Quest_Step
/// TODO: DELETE
/// </summary>
public class Press_Back_Quest_Step : Dialogue_Quest_Step
{
    void OnEnable()
    {
        // Game_Events_Manager.Instance.onRoomChange += RoomChange;
        SceneManager.sceneLoaded += onSceneLoaded;

    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        // Game_Events_Manager.Instance.onRoomChange -= RoomChange;
        SceneManager.sceneLoaded -= onSceneLoaded;
    }


    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Restaurant")
            FinishQuestStep();

    }
}
