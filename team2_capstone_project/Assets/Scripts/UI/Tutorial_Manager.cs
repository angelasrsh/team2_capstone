#define TUTORIAL_ENABLE // Comment this out to disable automatic tutorials

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
// using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///  Tutorial Manager (Singleton)
/// 
/// Provides controls to start, skip [WIP], or disable tutorials (#define above).
/// [Tentative] Stores and tracks which instructions and panels need to be displayed.
/// [Tentative] Controls a Canvas child object that can display tutorial text or highlight icons
/// 
/// ON USING THIS CLASS:
/// When adding new tutorials, you must 
///     1) Add their S.O.s to the TutorialList in the Inspector
///     2) In the matching position, add the tutorial's room to the TutorialRoomIDs list in the Inspector
/// 
/// </summary>
public class Tutorial_Manager : MonoBehaviour
{

    public static Tutorial_Manager Instance { get; private set; }

    ////// Variables that should be set in the Inspector //////

    // Quest S.O.s for tutorials. Update when new tutorials are added // TODO: Refactor into something better
    [Header("List of tutorials")]
    [SerializeField] private Quest_Info_SO[] TutorialList;
    [Header("Rooms corresponding to above tutorials")]

    [SerializeField] private Room_Data.RoomID[] TutorialRoomIDs;

   

    ////// Private variables //////
    private Dictionary<String, Quest_Info_SO> RoomTutorialMap = new Dictionary<String, Quest_Info_SO>();
    private Dictionary<String, Quest_State> QuestIDQuestStateMap = new Dictionary<String, Quest_State>(); // Map quest IDs to quest states

    //private int instructionIndex = 0;

    // Set variables
    private void Awake()
    {
        // Create singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Create a dictionary mapping Room IDs to tutorials
        if (TutorialList.Length != TutorialRoomIDs.Length)  // The tutorial and room list must match and be the same length
            Debug.LogWarning("[T_MAN] ERROR: Tutorial Manager tutorial list and Tutorial Room list are not the same length");

        for (int i = 0; i < TutorialList.Length; i++)
            RoomTutorialMap[TutorialRoomIDs[i].ToString()] = TutorialList[i]; // Map Tutorial S_O to RoomID key


    }

    // Subscribe to events
    private void OnEnable()
    {
        // For changes in quest states
        Game_Events_Manager.Instance.onQuestStateChange += questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange += ChangeQuestStep;

        // For detecting when we enter a room and need to start a tutorial
        SceneManager.sceneLoaded += CheckStartTutorial;
    }

    /// <summary>
    /// Called on the room change event. Starts a tutorial if we haven't completed it for that room yet.
    /// Will only start the tutorial if TUTORIAL_ENABLE is #defined.
    /// Add new tutorials using the instructions in Tutorial_Manager.
    /// </summary>
    /// <param name="roomID"> room being entered </param>
    private void CheckStartTutorial(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        // Don't auto-start tutorials if tutorials aren't enabled
#if !TUTORIAL_ENABLE
        return;
#endif
        // End search if this isn't a room that is listed to have a tutorial
        if (!RoomTutorialMap.ContainsKey(scene.name))
            return;

        String tutorialID = RoomTutorialMap[scene.name].id;

        // If a tutorial exists and is ready to start, start it
        if (QuestIDQuestStateMap.ContainsKey(tutorialID) && QuestIDQuestStateMap[tutorialID] == Quest_State.CAN_START)
        {
            //Debug.Log($"[T_MAN] Auto-starting tutorial {tutorialID}");
            StartTutorial(tutorialID);
        }
    }

    // Clean up by unsubscribing
    private void OnDisable()
    {
        Game_Events_Manager.Instance.onQuestStateChange -= questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange -= ChangeQuestStep;
    }

    /// <summary>
    /// Begin the tutorial (any quest) specified by the ID.
    /// [Not tested] This function will restart a finished quest. [Not tested] It also ignores the ENABLE_TUTORIAL #define 
    /// </summary>
    /// <param name="TutorialID"> ID of the quest to start </param>
    public void StartTutorial(String TutorialID)
    {
        Game_Events_Manager.Instance.StartQuest(TutorialID);
    }





    /// <summary>
    /// Update the internal quest state variables when the questStateChange event is broadcast.
    /// </summary>
    /// <param name="q"> ID of quest with the state change </param>
    private void questStateChange(Quest q)
    {
        // If we are tracking this quest's state, update our tutorial state dictionary
        if (RoomTutorialMap.ContainsValue(q.Info))
            QuestIDQuestStateMap[q.Info.id] = q.state;
    }

    /// <summary>
    /// Update the tutorial Canvas' text when the quest step changes
    /// </summary>
    /// <param name="id"> name of the quest </param>
    /// <param name="stepIndex"> The new quest step index </param>
    private void ChangeQuestStep(String id, int stepIndex)
    {
        // if (id.Equals(questID))
        // {
        //     instructionIndex = stepIndex; // Not necessary right now but may want to add a delay or embellishments later
        //     setText(questInfoForCanvas.dialogueList[stepIndex]);
        // }
    }

}
