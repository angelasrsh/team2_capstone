using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TODO: Merge with Tutorial Canvas
/// Per Cafe_Tutorial_Canvas:
/// Would like to delete this later when a better method is figured out
/// Will probably also want to delete this prefab, or at least edit it?
/// </summary>
public class Cafe_Tutorial_Canvas : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Quest_Info_SO questInfoForCanvas;

    private string questID;
    private Quest_State currentQuestState;
    private TextMeshProUGUI Textbox;

    private int instructionIndex = -1;
    
    // Temporary singleton- would like to get rid of this later.
    public static Cafe_Tutorial_Canvas Instance { get; private set; }

    private void Awake()
    {
        questID = questInfoForCanvas.id;
        Textbox = GetComponentInChildren<TextMeshProUGUI>();

        // Temporarily a Singleton; probably should be data persistence later
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

        void OnEnable()
        {
            Game_Events_Manager.Instance.onQuestStateChange += questStateChange;
            Game_Events_Manager.Instance.onQuestStepChange += ChangeQuestStep;
            Game_Events_Manager.Instance.onBeginDialogBox += BeginDialogueBox;
            Game_Events_Manager.Instance.onEndDialogBox += EndDialogBox;
            SceneManager.sceneLoaded += CheckScene;

            //currentQuestState = Quest_Manager.Instance.GetQuestByID(questID).state; // implement better method for getting state later
        //Game_Events_Manager.Instance.QuestStateChange()

        if (currentQuestState < Quest_State.IN_PROGRESS) // not great because it allows REQUIREMENTS_NOT_MET to start too
            Game_Events_Manager.Instance.StartQuest(questInfoForCanvas.id); // Start the quest immediately

        }

    void OnDisable()
    {
        Game_Events_Manager.Instance.onQuestStateChange -= questStateChange;
        Game_Events_Manager.Instance.onQuestStepChange -= ChangeQuestStep;
        Game_Events_Manager.Instance.onBeginDialogBox -= BeginDialogueBox;
        Game_Events_Manager.Instance.onEndDialogBox -= EndDialogBox;
        SceneManager.sceneLoaded -= CheckScene; // TODO: Find a better way later
        }

    // Update the Canvas's quest state when the quest changes
    private void questStateChange(Quest q)
    {
        if (q.Info.id.Equals(questID))
        {
            currentQuestState = q.state;
        }

        // Do nothing once tutorial is finished
        if (currentQuestState == Quest_State.FINISHED)
            this.gameObject.SetActive(false);


    }

    private void BeginDialogueBox()
    {
        GetComponent<Canvas>().enabled = false;
    }

    private void EndDialogBox()
    {
        GetComponent<Canvas>().enabled = true;
    }

    private void CheckScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals("Dating_Events") || scene.name.Equals("Main_Menu")) // TODO: Hard-coded- will change in next tutorial update
            GetComponent<Canvas>().enabled = false;
        else
            GetComponent<Canvas>().enabled = true;
    }

    /// <summary>
    /// Update the tutorial Canvas' text when the quest step changes
    /// </summary>
    /// <param name="id"> name of the quest </param>
    /// <param name="stepIndex"> The new quest step index </param>
    private void ChangeQuestStep(String id, int stepIndex)
    {
        if (id.Equals(questID))
        {
            instructionIndex = stepIndex; // Not necessary right now but may want to add a delay or embellishments later
            setText(questInfoForCanvas.dialogueList[stepIndex]);
        }
    }

    private void setText(String newText)
    {
        this.Textbox.text = newText;
    }
}
