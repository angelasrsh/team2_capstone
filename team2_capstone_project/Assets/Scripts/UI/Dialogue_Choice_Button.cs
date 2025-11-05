using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Dialogue_Choice_Button : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Component")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI choiceText;
    private int choiceIndex = -1;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

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
        // Force the button to show selected state
        button.OnSelect(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight when mouse hovers
        SelectButton();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle the click
        OnButtonClicked();
    }

    public void OnButtonClicked()
    {
        if (choiceIndex != -1)
        {
            Game_Events_Manager.Instance.dialogueEvents.UpdateChoiceIndex(choiceIndex);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (choiceIndex != -1)
        {
            Game_Events_Manager.Instance.dialogueEvents.UpdateChoiceIndex(choiceIndex);
        }
    }
}