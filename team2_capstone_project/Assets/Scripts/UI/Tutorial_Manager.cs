#define TUTORIAL_ENABLE // Comment this out to disable automatic tutorials

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Tutorial Manager (Singleton)
/// 
/// Provides controls to start, skip, or disable a tutorial.
/// Stores and tracks which instructions and panels need to be displayed.
/// Controls a Canvas child object that can display tutorial text or highlight icons
/// 
/// ON USING THIS CLASS: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
/// When adding new tutorials, you must 
///     1) Add their S.O.s to the variables in the class
///     2) Add a switch statement case in CheckStartTutorial for the roomChange event
///     3) Add a switch statement case for the questStateChange function
/// 
/// </summary>
public class Tutorial_Manager : MonoBehaviour
{

    ////// Variables that should be set in the Inspector //////

    // Quest S.O.s for tutorials. Update when new tutorials are added // TODO: Refactor into something better
    [SerializeField] private Quest_Info_SO WorldMapTutorial;
    [SerializeField] private Quest_Info_SO RestaurantTutorial;
    [SerializeField] private Quest_Info_SO CookingMinigameTutorial;
    [SerializeField] private Quest_Info_SO ChoppingMinigameTutorial;
    [SerializeField] private Canvas TutorialCanvas;
    

    ////// Private variables //////
    private Quest_State WorldMapTutorialQuestState; // NOTE: Are tutorials successfully independent?
    private Quest_State RestaurantTutorialQuestState;
    private Quest_State CookingTutorialQuestState;
    private Quest_State ChoppingTutorialQuestState;


    private int instructionIndex = 0;




    // Set variables
    private void Awake()
    {
        //questID = questInfoForCanvas.id;

    }

    // Subscribe to events
    private void OnEnable()
    {
        // For changes in quest states
        Game_Events_Manager.Instance.onQuestStateChange += questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange += ChangeQuestStep;

        // For detecting when we enter a room and need to start a tutorial
        Game_Events_Manager.Instance.onRoomChange += CheckStartTutorial;

        //currentQuestState = Quest_Manager.Instance.GetQuestByID(questID).state; // implement better method for getting state later
        //Game_Events_Manager.Instance.QuestStateChange()

        // Start the quest on the first visit to the world
        // if (currentQuestState < Quest_State.CAN_START) // not great because it allows REQUIREMENTS_NOT_MET to start too
        //     Game_Events_Manager.Instance.StartQuest(questInfoForCanvas.id); // Start the quest immediately

    }

    /// <summary>
    /// Called on the room change event. Starts a tutorial if we haven't completed it for that room yet.
    /// Will only start the tutorial if TUTORIAL_ENABLE is #defined.
    /// This is hard-coded to map tutorials to scenes: Add new tutorials by updating the switch statement.
    /// </summary>
    /// <param name="roomID"> room being entered </param>
    private void CheckStartTutorial(Room_Data.RoomID roomID)
    {
        // Don't auto-start tutorials if tutorials aren't enabled
#if !TUTORIAL_ENABLE
        return;
#endif

        // Start a tutorial if we haven't begun the tutorial for the entered scene yet // unsure about dealing with IN_PROGRESS tutorials
        switch (roomID)
        {
            case Room_Data.RoomID.World_Map:
                if (WorldMapTutorialQuestState == Quest_State.CAN_START)
                    StartTutorial("Foraging_Tutorial"); // ID using the Quest Scriptable Object ID
                break;
            case Room_Data.RoomID.Restaurant:
                if (RestaurantTutorialQuestState == Quest_State.CAN_START)
                    StartTutorial("Restaurant_Tutorial");
                break;
            case Room_Data.RoomID.Chopping_Minigame:
                if (ChoppingTutorialQuestState == Quest_State.CAN_START)
                    StartTutorial("Chopping_Tutorial");
                break;
            case Room_Data.RoomID.Cooking_Minigame:
                if (CookingTutorialQuestState == Quest_State.CAN_START)
                    StartTutorial("Caudron_Tutorial");
                break;
            default:
                Debug.Log($"[T_MAN] No tutorial found for roomID {roomID}");
                break;
        }


    }

    // Clean up by unsubscribing
    private void OnDisable()
    {
        Game_Events_Manager.Instance.onQuestStateChange -= questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange -= ChangeQuestStep;
    }

    /// <summary> // NOTE: IS THIS FUNCTION EVEN NECESSARY?
    /// Begin the tutorial (any quest) specified by the ID.
    /// This function will restart a finished quest. It also ignores the ENABLE_TUTORIAL #define
    /// </summary>
    /// <param name="TutorialID"> ID of the quest to start </param>
    public void StartTutorial(String TutorialID)
    {
        Game_Events_Manager.Instance.StartQuest(TutorialID);
    }





    /// <summary>
    /// Update the internal quest state variables when the questStateChange event is broadcast.
    /// NOTE: Add an if-statement for each tutorial added
    /// TODO: Refactor
    /// </summary>
    /// <param name="q"> ID of quest with the state change </param>
    private void questStateChange(Quest q)
    {

        if (q.Info.id.Equals(WorldMapTutorial.id)) // Foraging tutorial
        {
            WorldMapTutorialQuestState = q.state;
        }
        else if (q.Info.id.Equals(RestaurantTutorial.id)) // Restaurant tutorial
        {
            RestaurantTutorialQuestState = q.state;
        }
        else if (q.Info.id.Equals(CookingMinigameTutorial.id)) // Cauldron tutorial
        {
            CookingTutorialQuestState = q.state;
        }
        else if (q.Info.id.Equals(ChoppingMinigameTutorial.id)) // Cutting board tutorial
        {
            ChoppingTutorialQuestState = q.state;
        }
        else
        {
            Debug.Log($"[T_MAN] No tutorial case found for quest id {q.Info.id}");
        }

        // // Do nothing once tutorial is finished
        // if (currentQuestState == Quest_State.FINISHED)
        //     this.gameObject.SetActive(false);


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
