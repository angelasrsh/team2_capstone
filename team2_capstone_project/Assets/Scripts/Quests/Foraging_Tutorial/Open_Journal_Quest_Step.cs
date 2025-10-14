using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Journal_Quest_Step : Dialogue_Quest_Step
{
    [SerializeField] private string textKey;

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
            DelayedDialogue("Tutorial.Journal_PC", 3);
        } else
        {
            DelayedDialogue("Tutorial.Journal_Mobile", 3);
        }
    }


    private void JournalToggled(bool isOpen)
    {
        if (isOpen)
            FinishQuestStep(); // Finish and destroy this object
    }
}
