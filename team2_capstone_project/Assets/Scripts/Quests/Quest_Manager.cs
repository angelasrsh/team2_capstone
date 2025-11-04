using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Unity.VisualScripting;

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
/// 7) Toggle to refresh the Quest_Database so the manager can see it
/// 8) Make sure there is something that triggers the start of the quest (...StartQuest(...))
///    If it is a room-based tutorial, add it to the Tutorial_Manager to start when a certain room is first loaded
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

        foreach (Quest_Info_SO q in quest.Info.QuestPrerequisites)
        {
            if (GetQuestByID(q.id).state != Quest_State.FINISHED)
                meetsRequirements = false;

        }

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
                ChangeQuestState(q.Info.id, Quest_State.CAN_START);
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

        Save_Manager.instance?.AutoSave();
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
        UnlockQuests();
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
            q.IsPaused = false; // Assume not an inactive room (don't pause)

            for (int i = 0; i < q.Info.InactiveRooms.Count; i++)
            {
                // Is an active room - pause
                if (q.Info.InactiveRooms[i].ToString().Equals(scene.name))
                {
                    q.IsPaused = true;
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

    #region Save / Load
    public Quest_Manager_Data GetSaveData()
    {
        Quest_Manager_Data data = new Quest_Manager_Data();

        foreach (var kvp in questMap)
        {
            Quest q = kvp.Value;
            Quest_Save_Data qData = new Quest_Save_Data
            {
                questID = q.Info.id,
                state = q.state,
                currentStepIndex = q.CurrentStepIndex
            };
            data.allQuestData.Add(qData);
        }

        return data;
    }

    public void LoadFromSaveData(Quest_Manager_Data data)
    {
        if (data == null || data.allQuestData.Count == 0)
        {
            Debug.Log("[Q_MAN] No quest data to load; initializing defaults.");
            return;
        }

        foreach (var qData in data.allQuestData)
        {
            if (questMap.TryGetValue(qData.questID, out Quest quest))
            {
                quest.state = qData.state;
                quest.CurrentStepIndex = qData.currentStepIndex;

                // Recreate quest step if currently in progress
                if (quest.state == Quest_State.IN_PROGRESS && quest.CurrentStepExists())
                {
                    quest.InstantiateCurrentQuestStep(this.transform);
                }
            }
            else
            {
                Debug.LogWarning($"[Q_MAN] Quest ID {qData.questID} not found when loading.");
            }
        }

        Debug.Log("[Q_MAN] Quest data loaded successfully.");
    }
    #endregion
}

#region Quest_Save_Data
[System.Serializable]
public class Quest_Save_Data
{
    public string questID;
    public Quest_State state;
    public int currentStepIndex;
}
#endregion

#region Quest_Manager_Data
[System.Serializable]
public class Quest_Manager_Data
{
    public List<Quest_Save_Data> allQuestData = new List<Quest_Save_Data>();
}
#endregion


