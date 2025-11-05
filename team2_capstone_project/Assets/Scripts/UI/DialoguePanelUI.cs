using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialoguePanelUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public Dialog_UI_Manager uiManager;

    private void Awake()
    {
        contentParent.SetActive(false);
        ResetPanel();
    }

    private void OnEnable()
    {
        Game_Events_Manager.Instance.dialogueEvents.onDialogueStarted += DialogueStarted;
        Game_Events_Manager.Instance.dialogueEvents.onDialogueFinished += DialogueFinished;
        Game_Events_Manager.Instance.dialogueEvents.onDisplayDialogue += DisplayDialogue;

    }

    private void OnDisable()
    {
        Game_Events_Manager.Instance.dialogueEvents.onDialogueStarted -= DialogueStarted;
        Game_Events_Manager.Instance.dialogueEvents.onDialogueFinished -= DialogueFinished;
        Game_Events_Manager.Instance.dialogueEvents.onDisplayDialogue -= DisplayDialogue;

    }
    private void DialogueStarted()
    {
        contentParent.SetActive(true);
        
    }

    private void DialogueFinished()
    {
        contentParent.SetActive(false);
        ResetPanel();
    }
    bool disablePlayerInput = true;

    private void DisplayDialogue(string dialogueLine)
    {
        dialogueText.text = dialogueLine;
        contentParent.SetActive(true);

    }
    private void ResetPanel()
    {
        dialogueText.text = "";
    }
}
