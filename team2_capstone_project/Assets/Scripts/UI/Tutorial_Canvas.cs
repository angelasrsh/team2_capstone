using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Tutorial_Canvas : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Quest_Info_SO questInfoForCanvas;

    private string questID;
    private Quest_State currentQuestState;
    private TextMeshProUGUI Textbox;

    private int instructionIndex = -1;

    private void Awake()
    {
        questID = questInfoForCanvas.id;
        Textbox = GetComponentInChildren<TextMeshProUGUI>();
    }
    void OnEnable()
    {
        Game_Events_Manager.Instance.onQuestStateChange += questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange += ChangeQuestStep;

        Game_Events_Manager.Instance.StartQuest(questInfoForCanvas.id); // Start the quest immediately

    }
    
       void OnDisable()
    {
        Game_Events_Manager.Instance.onQuestStateChange -= questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange -= ChangeQuestStep;
    }

    // Update the Canvas's quest state when the quest changes
    private void questStateChange(Quest q)
    {
        if (q.Info.id.Equals(questID))
        {
            currentQuestState = q.state;
        }

        // Do nothing once tutorial is finished
        if (currentQuestState == Quest_State.FINISHED)
            this.gameObject.SetActive(false);
        

    }

    /// <summary>
    /// Update the tutorial Canvas' text when the quest step changes
    /// </summary>
    /// <param name="id"> name of the quest </param>
    /// <param name="stepIndex"> The new quest step index </param>
    private void ChangeQuestStep(String id, int stepIndex)
    {
        if (id.Equals(questID))
        {
            instructionIndex = stepIndex; // Not necessary right now but may want to add a delay or embellishments later
            setText(questInfoForCanvas.dialogueList[stepIndex]);
        }
    }

    private void setText(String newText)
    {
        this.Textbox.text = newText;
    }
}
