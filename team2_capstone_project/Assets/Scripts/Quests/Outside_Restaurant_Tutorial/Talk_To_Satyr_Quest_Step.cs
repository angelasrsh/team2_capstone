using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Tutorial Quest Step to make the player sprint
/// </summary>
public class Talk_To_Satyr_Quest_Step : Dialogue_Quest_Step
{

    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onInShop += InShop;
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onInShop -= InShop;
    }

     void Start()
    {
        DelayedDialogue(10, 0, false);
    }



    private void InShop(bool isInShop)
    {
        if (isInShop)
            FinishQuestStep(); // Finish and destroy this object
    }





}
