using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// The child canvas used by the Tutorial_Manager. 
/// Provides functions to be called by the manager
/// And a set of panels for highlighting certain parts of the screen for certain tutorial steps.
/// 
/// ON USING THIS CLASS:
/// Call these functions from Tutorial_Manager if you want to display UI stuff during a tutorial.
/// </summary>
public class Tutorial_Canvas : MonoBehaviour
{
    [SerializeField] private GameObject TextboxPanel; // Set in code eventuallyS
    [SerializeField] private GameObject HighlightPanel;

    private TextMeshProUGUI Textbox;

    // List of panels that must be in the desired order
    //private GameObject TutorialPanels;


    private void Awake()
    {
        // Set textbox and Tutorial Panels list
        Textbox = TextboxPanel.GetComponentInChildren<TextMeshProUGUI>();

    }

    /// <summary>
    /// Set the tutorial textbox to a new string
    /// </summary>
    /// <param name="newText"></param>
    private void setText(String newText)
    {
        Textbox = TextboxPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (Textbox == null)
            Debug.Log("[Tu_CAN] Cannot set textbox because it is null! Has it been initialized yet?");
        Textbox.text = newText;
    }

    // /// <summary>
    // /// Called by Tutorial_Manager to display a tutorial text string and its background textbox 
    // /// </summary>
    // /// <param name="newText"> Text to display</param>
    // /// <param name="delayStart"> Seconds to wait before displaying; default 0 </param>
    // /// <param name="delayHide"> Seconds to wait before hiding. Text will remain on screen if delayHide is 0 or unused </param>
    // public void DisplayText(String newText, float delayStart = 0, float delayHide = 0)
    // {
    //     // Wait for delayStart, then show text and textbox
    //     Debug.Log("Displaying text: " + newText);

    //     // Hide after delayHide seconds. Leave visible if delayHide is 0

    // }

    /// <summary>
    /// Called by Tutorial_Manager to display a tutorial text string and its background textbox 
    /// </summary>
    /// <param name="newText"> Text to display</param>
    public void DisplayText(String newText)
    {
        // Enable panel
        TextboxPanel.SetActive(true);
        setText(newText);
        Debug.Log("Displaying text: " + newText);
    }

    public void DisplayHighlight()
    {
        HighlightPanel.SetActive(true);
    }
}
