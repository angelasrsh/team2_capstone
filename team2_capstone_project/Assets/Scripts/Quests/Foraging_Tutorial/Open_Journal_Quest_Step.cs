using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Journal_Quest_Step : Dialogue_Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onJournalToggle += JournalToggled;
       
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onJournalToggle -= JournalToggled;
    }

    private void Start()
    {

        // If PC, choose key that refers to the keybind instead of the icon
        if (SystemInfo.deviceType != DeviceType.Handheld && !simulateMobile)
        {
            DelayedDialogue(3, 0, false);
        } else
        {
            DelayedDialogue(3, 0, false);
        }
    }


    private void JournalToggled(bool isOpen)
    {
        if (isOpen)
            FinishQuestStep(); // Finish and destroy this object
    }
}
