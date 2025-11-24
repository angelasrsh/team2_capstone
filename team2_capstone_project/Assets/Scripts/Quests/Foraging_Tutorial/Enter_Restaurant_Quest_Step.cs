using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enter_Restaurant_Quest_Step : Dialogue_Quest_Step
{
    [SerializeField] private Quest_Info_SO FinishQuestToPrompt; // Intended to prompt after talking to Satyr

    // Start is called before the first frame update
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onRoomChange += EnterRestaurant;
        Game_Events_Manager.Instance.onInShop += PromptEnterRestaurant;
        // make sure to nblock area
        // DelayedDialogue(0, 0, false);

    }

    // Update is called once per frame
    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onRoomChange -= EnterRestaurant;
        Game_Events_Manager.Instance.onInShop -= PromptEnterRestaurant;

    }

    void Start()
    {
        Game_Events_Manager.Instance.SetExitsBlocked(false);
    }

    void PromptEnterRestaurant(bool inShop)
    {
        if (!inShop && Quest_Manager.Instance.GetQuestByID(FinishQuestToPrompt.id).state == Quest_State.FINISHED)
            DelayedDialogue(1, 0, false);
        
    }



    /// <summary>
    /// End quest when player exits to a non-main-menu scene
    /// </summary>
    /// <param name="currentRoom"> RoomID of the room being left </param>
    /// <param name="exitingTo"> RoomID of the room being entered </param>
    void EnterRestaurant(Room_Data.RoomID currentRoom, Room_Data.RoomID exitingTo)
    {
        if (exitingTo == Room_Data.RoomID.Updated_Restaurant)
            FinishQuestStep();

    }
}
