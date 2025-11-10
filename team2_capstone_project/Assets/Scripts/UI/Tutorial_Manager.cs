#define TUTORIAL_ENABLE // Comment this out to disable automatic tutorials

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
// using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
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
///     3) Make sure the Quest is in the Quest Database
///     4) Make sure the scene has a Quest_Manager, Tutorial_Manager, and Dialog_UI
///     5) Create and add all quest step prefabs you want to the quest S.O.
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
    private Dictionary<Quest_Info_SO, String> RoomTutorialMap = new Dictionary<Quest_Info_SO, String>();
    private Dictionary<String, Quest_State> QuestIDQuestStateMap = new Dictionary<String, Quest_State>(); // Map quest IDs to quest states


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

        // Initialize room tutorial map
        for (int i = 0; i < TutorialList.Length; i++)
            RoomTutorialMap[TutorialList[i]] = TutorialRoomIDs[i].ToString(); // Map Tutorial S_O to RoomID key

        for (int i = 0; i < TutorialList.Length; i++)
        {
            QuestIDQuestStateMap[TutorialList[i].id] = Quest_State.REQUIREMENTS_NOT_MET;
            QuestIDQuestStateMap[TutorialList[i].id] = Quest_Manager.Instance.GetQuestByID(TutorialList[i].id).state;
            // Helpers.printLabeled(this, TutorialList[i].id + QuestIDQuestStateMap[TutorialList[i].id].ToString());
        }
            

    }

    // Subscribe to events
    private void OnEnable()
    {
        // For changes in quest states
        Game_Events_Manager.Instance.onQuestStateChange += questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange += ChangeQuestStep;

        // State tracking for now
        Game_Events_Manager.Instance.onInventoryToggle += InventoryToggle;

        // For detecting when we enter a room and need to start a tutorial
        SceneManager.sceneLoaded += CheckStartTutorial;

        CheckStartTutorial(SceneManager.GetActiveScene());
    }

    
    // Clean up by unsubscribing
    private void OnDisable()
    {
        Game_Events_Manager.Instance.onQuestStateChange -= questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange -= ChangeQuestStep;

        // State tracking for now
        Game_Events_Manager.Instance.onInventoryToggle += InventoryToggle;
    }

    /// <summary>
    /// Called on the room change event. Starts a tutorial if we haven't completed it for that room yet.
    /// Will only start the tutorial if TUTORIAL_ENABLE is #defined.
    /// Add new tutorials using the instructions in Tutorial_Manager.
    /// </summary>
    /// <param name="roomID"> room being entered </param>
    private void CheckStartTutorial(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        // Don't auto-start tutorials if tutorials aren't enabled
#if !TUTORIAL_ENABLE
        return;
#endif
        //Helpers.printLabeled(this, "");
        // End search if this isn't a room that is listed to have a tutorial
        if (!RoomTutorialMap.ContainsValue(scene.name))
            return;

        // Start all tutorials with the given room start ID
        foreach (var kvp in RoomTutorialMap)
        {
            String tutorialID = kvp.Key.id;

            // If the room matches and the tutorial quest is ready to start, start it
            if (kvp.Value == scene.name && QuestIDQuestStateMap[tutorialID] == Quest_State.CAN_START)
            {
                //Debug.Log($"[T_MAN] Auto-starting tutorial {tutorialID}");
                StartTutorial(tutorialID);
                // Helpers.printLabeled(this, "Starting" + tutorialID);
            }

        }
        
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
        if (RoomTutorialMap.ContainsKey(q.Info))
        {
            QuestIDQuestStateMap[q.Info.id] = q.state;
        }

        CheckStartTutorial(SceneManager.GetActiveScene(), LoadSceneMode.Single); // check if we can start any new tutorial quests
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

    #region State_Variables_For_Persistence_For_Now
    /// State variables that are here until I have a better way of saving info

    public bool hasOpenedInventory = false;
    public bool hasClosedInventory = false;

    private void InventoryToggle(bool isOpen)
    {
        if (isOpen)
            hasOpenedInventory = true;
        else if (hasOpenedInventory)
            hasClosedInventory = false;
    }
    
    #endregion

}
