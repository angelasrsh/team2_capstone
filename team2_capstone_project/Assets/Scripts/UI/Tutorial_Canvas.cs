// using System; ////////////// TODO: Delete
// using System.Collections;
// using System.Collections.Generic;
// using JetBrains.Annotations;
// using TMPro;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// /// <summary>
// /// The child canvas used by the Tutorial_Manager. 
// /// Provides functions to be called by the manager
// /// And a set of panels for highlighting certain parts of the screen for certain tutorial steps.
// /// 
// /// ON USING THIS CLASS:
// /// Call these functions from Tutorial_Manager if you want to display UI stuff during a tutorial.
// /// </summary>
// public class Tutorial_Canvas : MonoBehaviour
// {
//     [SerializeField] private Room_Data.RoomID TutorialRoom;


//     [SerializeField] private GameObject TextboxPanel; // Set in code eventuallyS
//     [SerializeField] private GameObject HighlightPanel;

//     private TextMeshProUGUI Textbox;

//     private bool popupAlreadySet = false;

//     // List of panels that must be in the desired order
//     //private GameObject TutorialPanels;

//     private void OnEnable()
//     {
//         SceneManager.sceneLoaded += checkDisplayCanvas;

//         Game_Events_Manager.Instance.onBeginDialogBox += BeginDialogueBox;
//         Game_Events_Manager.Instance.onEndDialogBox += EndDialogBox;

//         TutorialRoom = Room_Manager.GetRoomFromActiveScene().roomID;

//     }

//     private void OnDisable()
//     {
//         SceneManager.sceneLoaded -= checkDisplayCanvas;

//         Game_Events_Manager.Instance.onBeginDialogBox -= BeginDialogueBox;
//         Game_Events_Manager.Instance.onEndDialogBox -= EndDialogBox;
//     }


//     private void Awake()
//     {
//         // Set textbox and Tutorial Panels list
//         Textbox = TextboxPanel.GetComponentInChildren<TextMeshProUGUI>();

//     }

//     private void checkDisplayCanvas(Scene scene, LoadSceneMode mode)
//     {
//         if (scene.name.Equals(TutorialRoom.ToString()))
//         {
//             TextboxPanel.SetActive(true);

//             if (popupAlreadySet) // Only enable if the player got to the popup in the past
//                 HighlightPanel.SetActive(true);
//         }
//         else
//         {
//             DisableAll();
//         }

//     }



//     /// <summary>
//     /// Called by a quest step to display a tutorial text string and its background textbox 
//     /// </summary>
//     /// <param name="newText"> Text to display</param>
//     /// <param name="delayStart"> Seconds to wait before displaying; default 0 </param>
//     /// <param name="delayHide"> Seconds to wait before hiding. Text will remain on screen if delayHide is 0 or unused </param>
//     public void DisplayTextDelayed(String newText, float delayStart = 0, float delayHide = 0)
//     {
//         // Wait for delayStart, then show text and textbox, then disappear
//         StartCoroutine(displayTextDelayed(newText, delayStart, delayHide));
//     }

//     public void DisplayGraphicDelayed(float delayStart = 0, float delayHide = 0)
//     {
//         StartCoroutine(displayGraphicDelayed(delayStart, delayHide));

//     }

//     public void DisableAll()
//     {
//         TextboxPanel.SetActive(false);
//         HighlightPanel.SetActive(false);
//     }


//     /// <summary>
//     /// Change this quest step's canvas text after delayStart time. Hide after delayHide time.
//     /// </summary>
//     /// <param name="text"></param>
//     /// <param name="delayStart"></param>
//     /// <param name="delayHide"> Input 0 to never hide </param>
//     /// <returns></returns>
//     IEnumerator displayTextDelayed(String text, float delayStart, float delayHide)
//     {
//         yield return new WaitForSeconds(delayStart);
//         setText(text);
//         TextboxPanel.SetActive(true);
//         if (delayHide > 0)
//         {
//             yield return new WaitForSeconds(delayHide);
//             TextboxPanel.SetActive(false);
//         }
//     }


//     /// <summary>
//     /// Called by Tutorial_Manager to display a tutorial text string and its background textbox 
//     /// </summary>
//     /// <param name="newText"> Text to display</param>
//     // public void DisplayText(String newText)
//     // {
//     //     // Enable panel
//     //     TextboxPanel.SetActive(true);
//     //     setText(newText);
//     //     Debug.Log("Displaying text: " + newText);
//     // }

//     /// <summary>
//     /// Change this quest step's canvas to display a graphic after delayStart time. Hide after delayHide time.
//     /// </summary>
//     /// <param name="delayStart"></param>
//     /// <param name="delayHide"> Input 0 to never hide </param>
//     /// <returns></returns>
//     IEnumerator displayGraphicDelayed(float delayStart, float delayHide)
//     {
//         yield return new WaitForSeconds(delayStart);
//         HighlightPanel.SetActive(true);
//         popupAlreadySet = true;
//         if (delayHide > 0)
//         {
//             yield return new WaitForSeconds(delayHide);
//             HighlightPanel.SetActive(false);
//         }

//     }

//     // public void DisplayHighlight()
//     // {
//     //     if (HighlightPanel == null)
//     //         Debug.Log("[TU_CAN] Cannot display highlight- panel is null. Please assign it in the inspector.");
//     //     else

//     // }


//     /// <summary>
//     /// Set the tutorial textbox to a new string
//     /// </summary>
//     /// <param name="newText"></param>
//     private void setText(String newText)
//     {
//         Textbox = TextboxPanel.GetComponentInChildren<TextMeshProUGUI>();
//         if (Textbox == null)
//             Debug.Log("[Tu_CAN] Cannot set textbox because it is null! Has it been initialized yet?");
//         Textbox.text = newText;
//     }

//     private void BeginDialogueBox(string dialogKey)
//     {
//         TextboxPanel.SetActive(false);
//     }

//     private void EndDialogBox(string dialogKey)
//     {
//         TextboxPanel.SetActive(true);
//     }
    
    
// }
