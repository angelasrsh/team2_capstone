using System.Collections;
using System.Collections.Generic;
using Ink.Parsed;
using TMPro;
using UnityEngine;
using Ink.Runtime;

public class DialoguePanelUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public Dialog_UI_Manager uiManager;
    [SerializeField] private Dialogue_Choice_Button[] choiceButtons;

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

    private void DisplayDialogue(string dialogueLine, List<Ink.Runtime.Choice> dialogueChoices)
    {
        Debug.Log("DisplayDialog is working!");
        dialogueText.text = dialogueLine;
        if (dialogueChoices.Count > choiceButtons.Length)
        {
            Debug.Log("More dialogue choices ("
                + dialogueChoices.Count + ") came through than are supported ("
                + choiceButtons.Length + ").");
        }

        //increment choice index and decremenet choice button index
        int choiceButtonIndex = dialogueChoices.Count - 1;
        for (int inkChoiceIndex = 0; inkChoiceIndex < dialogueChoices.Count; inkChoiceIndex++)
        {
            Ink.Runtime.Choice dialogueChoice = dialogueChoices[inkChoiceIndex];
            Dialogue_Choice_Button choiceButton = choiceButtons[choiceButtonIndex];

            choiceButton.gameObject.SetActive(true);
            choiceButton.SetChoiceText(dialogueChoice.text);
            choiceButton.SetChoiceIndex(inkChoiceIndex);

            if (inkChoiceIndex == 0)
            {
                choiceButton.SelectButton();
                Game_Events_Manager.Instance.dialogueEvents.UpdateChoiceIndex(0);
            }
            choiceButtonIndex--;
        }

    }
    private void ResetPanel()
    {
        dialogueText.text = "";
    }
}
