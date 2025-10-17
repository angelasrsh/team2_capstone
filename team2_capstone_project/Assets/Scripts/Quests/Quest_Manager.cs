using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Credit for the quest system goes to https://www.youtube.com/watch?v=UyTJLDGcT64 (with modification)

/// <summary>
/// Holds data for all quests.
/// 
/// To add a new quest
/// 1) Create a Quest_Info_SO for the quest
/// 2) Make a folder in the Scripts/Quests folder to hold all the quest step scripts
/// 3) Extend the Quest_Step class and write the logic for each step
///   (Can reference existing steps in the Quests->Foraging_Tutorial folder)
/// 4) Make a folder in Prefabs/Quests to hold quest step prefabs that will use step scripts
/// 5) Create a GameObject prefab with a Quest_Step for each step in the quest
/// 6) Add the Quest_Step prefabs to the Quest_Info_SO for the quest
/// 7) Add the Quest_Info_SO to the Quest_Database so the manager can see it
/// 8) Make sure there is something that triggers the start of the quest (...StartQuest(...))
///    If it is a tutorial, add it to the Tutorial_Manager to start when a certain room is first loaded
/// </summary>
public class Quest_Manager : MonoBehaviour
{
    private Dictionary<string, Quest> questMap;

    public Quest_Database QuestDatabase;

    // For singleton purposes
    public static Quest_Manager Instance { get; private set; }

    private void Awake()
    {
        questMap = CreateQuestMap();

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


    private void OnEnable()
    {
        // Subscribe to quest notifications
        Game_Events_Manager.Instance.onStartQuest += StartQuest;
        Game_Events_Manager.Instance.onAdvanceQuest += AdvanceQuest;
        Game_Events_Manager.Instance.onFinishQuest += FinishQuest;

        Game_Events_Manager.Instance.onQuestStepChange += QuestStepChange;

        SceneManager.sceneLoaded += CheckPauseQuests;

        UnlockQuests();


    }

    private void OnDisable()
    {
        // Unsubscribe to quest notifications
        Game_Events_Manager.Instance.onStartQuest -= StartQuest;
        Game_Events_Manager.Instance.onAdvanceQuest -= AdvanceQuest;
        Game_Events_Manager.Instance.onFinishQuest -= FinishQuest;

        Game_Events_Manager.Instance.onQuestStepChange -= QuestStepChange;
        
        SceneManager.sceneLoaded -= CheckPauseQuests;
    }

    /// <summary>
    /// Called by <XXXXXXXXXXXXXXXXXXXXX> to tell the events manager to broadcast
    /// a QuestStateChange announcement to all listening objects
    /// </summary>
    /// <param name="id"> quest ID to update </param>
    /// <param name="state"> the new state </param>
    private void ChangeQuestState(string id, Quest_State state)
    {
        Quest quest = GetQuestByID(id);
        quest.state = state;
        Game_Events_Manager.Instance.QuestStateChange(quest);
    }

    /// <summary>
    /// Called by Quest_Manager in Update to see if we can start a quest
    /// </summary>
    /// <param name="quest"> Quest to check</param>
    /// <returns></returns>
    private bool CheckRequirementsMet(Quest quest)
    {
        bool meetsRequirements = true;

        // Can later check requirements and set meetsRequirements to false if not met

        return meetsRequirements;
    }

    /// <summary>
    /// Check all quests in the database and set the state to CAN_START if requirements have been met
    /// </summary>
    public void UnlockQuests()
    {
        foreach (Quest q in questMap.Values)
        {
            if (q.state == Quest_State.REQUIREMENTS_NOT_MET && CheckRequirementsMet(q))
                ChangeQuestState(q.Info.id, Quest_State.CAN_START); // May later want to use CAN_START to not auto-start
        }
    }

    private void StartQuest(string id)
    {
        // Debug.Log($"[Q_MAN] started quest {id}");
        Quest quest = GetQuestByID(id);
        quest.InstantiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.Info.id, Quest_State.IN_PROGRESS);
    }

    private void AdvanceQuest(string id)
    {
        // Debug.Log($"Q_MAN advanced quest {id}");
        Quest quest = GetQuestByID(id);
        quest.MoveToNextStep();
        if (quest.CurrentStepExists())
            quest.InstantiateCurrentQuestStep(this.transform);
        else
            ChangeQuestState(quest.Info.id, Quest_State.FINISHED); // or add CAN_FINISHED if not auto-finishing
    }

    /// <summary>
    /// Maybe not necessary for now if quests auto-finish?
    /// </summary>
    /// <param name="id"></param>
    private void FinishQuest(string id)
    {
        // Debug.Log($"Q_MAN finished quest {id}");
        Quest quest = GetQuestByID(id);
        // claim rewards if applicable
        ChangeQuestState(quest.Info.id, Quest_State.FINISHED);
    }

    /// <summary>
    /// Broadcast that a quest step has changed
    /// </summary>
    /// <param name="id"> The id of the quest </param>
    /// <param name="stepIndex"> The new index of the quest </param>
    private void QuestStepChange(string id, int stepIndex)
    {
        Quest quest = GetQuestByID(id);
        ChangeQuestState(id, quest.state); // Re-broadcast quest with the same state (only step has changed)
        // Debug.Log($"[Q_MAN] Quest Step Change {id} {quest.state} to step {stepIndex}");
    }

    /// <summary>
    /// Check all quests and pause/activate them based on the room we're in
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void CheckPauseQuests(Scene scene, LoadSceneMode mode)
    {
        foreach (KeyValuePair<string, Quest> questkvp in questMap)
        {
            Quest q = questkvp.Value;
            q.IsPaused = true; // Assume not an active room (pause)

            for (int i = 0; i < q.Info.ActiveRooms.Count; i++)
            {
                // Is an active room - unpause
                if (q.Info.ActiveRooms[i].ToString().Equals(scene.name))
                {
                    q.IsPaused = false;
                }

            }

            Game_Events_Manager.Instance.SetQuestPaused(q.Info.id, q.IsPaused);



        }


    }

    /// <summary>
    /// Used by the Quest_Manager to make a map that returns a quest when given its ID
    /// </summary>
    /// <returns> A populated quest map to fill the manager's map </returns>
    private Dictionary<string, Quest> CreateQuestMap()
    {
        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
        foreach (Quest_Info_SO questInfo in QuestDatabase.allQuests)
        {
            if (idToQuestMap.ContainsKey(questInfo.id))
                Debug.LogWarning($"[Q_Man] Duplicate quest ID found when creating quest map: {questInfo.id}");
            idToQuestMap.Add(questInfo.id, new Quest(questInfo));
        }

        return idToQuestMap;
    }

    /// <summary>
    /// Used by the quest manager to get a quest from its id.
    /// Improves on directly accessing the dictionary by adding null-checking
    /// </summary>
    /// <param name="id"> The quest ID for which to search </param>
    /// <returns></returns>
    public Quest GetQuestByID(string id) // Make private and decouple from Tutorial Canvas once we have data persistence
    {
        Quest quest = questMap[id];
        if (quest == null)
            Debug.LogError($"[Q_Man] Quest id {id} not found in quest map");
        return quest;
    }

}
