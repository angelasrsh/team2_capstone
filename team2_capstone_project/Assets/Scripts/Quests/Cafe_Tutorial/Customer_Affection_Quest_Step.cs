using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer_Affection_Quest_Step : Dialogue_Quest_Step
{
    public CustomerData customerToClick;
    override protected void OnEnable()
    {
        // Game_Events_Manager.Instance.onJournalToggle += JournalToggled;
        Game_Events_Manager.Instance.onCustomerDetailsClick += NPCClick;
        // Game_Events_Manager.Instance.onDialogueComplete += onDialogComplete;
        // Game_Events_Manager.Instance.onEndDialogBox += endQuest;

        base.OnEnable();
    }

    // Unsubscribe to clean up
    override protected void OnDisable()
    {
        // Game_Events_Manager.Instance.onJournalToggle -= JournalToggled;
        Game_Events_Manager.Instance.onCustomerDetailsClick -= NPCClick;
        // Game_Events_Manager.Instance.onDialogueComplete -= onDialogComplete;
        // Game_Events_Manager.Instance.onEndDialogBox -= endQuest;

        base.OnDisable();
    }

    private void Start()
    {
        DelayedDialogue(0, 0, false);
    }

    
   
   
    #region Events

//     /// <summary>
//     /// Step 2- click on recipe
//     /// Note that NPC has been clicked and finish quest step after some amount of time
//     /// </summary>
//     /// <param name="customerData"></param>
    private void NPCClick(CustomerData customerData)
    {
        if (customerData == customerToClick)
        {

            // Manually close any open dialogue box
            if (dm != null)
            {
                Debug.Log("[Tutorial] Manually ending current dialogue.");
                dm.EndDialog();
            }
            DelayedDialogue(0, 0, false, "Journal.Asper_Affection");
            WaitFinishQuestStep(5);
        }
    }
    #endregion
}
