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
    private TextMeshProUGUI Text;

    //private String[] instructions = { "Move using WASD or the arrow keys", "Press J to open the journal and see the ingredients and recipes you know", "Press I to see your inventory", "Go to an object and press E to harvest it" };
    private int instructionIndex = 0;

    private void Awake()
    {
        questID = questInfoForCanvas.id;
        Text = GetComponentInChildren<TextMeshProUGUI>();
    }
    void OnEnable()
    {
        //Game_Events_Manager.Instance.onQuestStateChange += questStateChange;
        Game_Events_Manager.Instance.StartQuest(questInfoForCanvas.id);
        Game_Events_Manager.Instance.onQuestStepChange += ChangeQuestStep;

    }

    // Update the Canvas's quest state when the quest changes
    private void questStateChange(Quest q)
    {
        if (q.Info.id.Equals(questID))
            currentQuestState = q.state;
    }

    /// <summary>
    /// Update the tutorial Canvas' text when the quest step changes
    /// </summary>
    /// <param name="id"> name of the quest </param>
    /// <param name="stepIndex"> The new quest step index </param>
    private void ChangeQuestStep(String id, int stepIndex)
    {
        if (id.Equals(questID) && currentQuestState == Quest_State.IN_PROGRESS)
        {
            instructionIndex = stepIndex; // Not necessary right now but may want to add a delay or embellishments later

            if (instructionIndex < questInfoForCanvas.dialogueList.Length)
                setText(questInfoForCanvas.dialogueList[instructionIndex]);
        }
    }

    private void setText(String newText)
    {
        this.Text.text = newText;
    }
}
