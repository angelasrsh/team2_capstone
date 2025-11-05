using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elf_Ring_Quest_Step : Quest_Step
{
    [SerializeField] private string dialogueKnotName;
    protected void OnEnable()
    {
        Game_Events_Manager.Instance.onOverworldNPCDialogue += Elf75Dialogue;
    }
    
    protected void OnDisable()
    {
        Game_Events_Manager.Instance.onOverworldNPCDialogue -= Elf75Dialogue;
    }

    private void Start()
    {   

    }   

    private void Elf75Dialogue()
    {
        // if (isOpen)
        Debug.Log("Insert dialogue for elf here");
        if(!dialogueKnotName.Equals(""))
        {
            Game_Events_Manager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
        }
            FinishQuestStep(); // Finish and destroy this object
    }
}
