using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Dialogue_Choice_Button : MonoBehaviour, ISelectHandler
{
    [Header("Component")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI choiceText;
    private int choiceIndex = -1;

    public void SetChoiceText(string choiceTextString)
    {
        choiceText.text = choiceTextString;
    }

    public void SetChoiceIndex(int choiceIndex)
    {
        this.choiceIndex = choiceIndex;
    }
    public void SelectButton()
    {
        button.Select();
    }

    public void OnSelect(BaseEventData eventData)
    {
        Game_Events_Manager.Instance.dialogueEvents.UpdateChoiceIndex(choiceIndex);
    }
}
