using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Tutorial Quest Step to make the player move using WASD or arrow key binds
/// </summary>
public class Move_WASD_Quest_Step : Quest_Step
{
    // Testing- will clean up later!
    public TextMeshPro text;

    void OnEnable()
    {
        Game_Events_Manager.Instance.onPlayerMove += PlayerMoved;

        // Testing!!!
        text = GameObject.Find("Tutorial_Canvas").GetComponent<TextMeshPro>();

        if (text != null)
            text.text = "Press WASD or the arrow keys to move";
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onPlayerMove -= PlayerMoved;
    }


    private void PlayerMoved()
    {
        FinishQuestStep(); // Finish and destroy this object
    }
}
