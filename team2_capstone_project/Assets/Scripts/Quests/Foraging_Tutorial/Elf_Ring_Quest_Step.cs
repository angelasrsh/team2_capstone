using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elf_Ring_Quest_Step : Quest_Step
{
    [SerializeField] public string dialogueKnotName;
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

            // Debug.Log("PlayScene New Ink in Elf75Dialogue");

            if (!dialogueKnotName.Equals(""))
            {
                Game_Events_Manager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
            }
            else
            {
                Debug.LogWarning("[Foraging_Area_NPC_Actor] No dialogue knot name found!");
            }
            FinishQuestStep(); // Finish and destroy this object
    }
}
