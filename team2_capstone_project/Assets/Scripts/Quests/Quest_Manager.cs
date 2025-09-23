using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
/// </summary>
public class Quest_Manager : MonoBehaviour
{
    private Dictionary<string, Quest> questMap;

    public Quest_Database QuestDatabase;

    private void Awake()
    {
        questMap = CreateQuestMap();
    }

    private void OnEnable()
    {
        // Subscribe to quest notifications
        Game_Events_Manager.Instance.onStartQuest += StartQuest;
        Game_Events_Manager.Instance.onAdvanceQuest += AdvanceQuest;
        Game_Events_Manager.Instance.onFinishQuest += FinishQuest;
    }

     private void OnDisable()
    {
        // Unsubscribe to quest notifications
        Game_Events_Manager.Instance.onStartQuest -= StartQuest;
        Game_Events_Manager.Instance.onAdvanceQuest -= AdvanceQuest;
        Game_Events_Manager.Instance.onFinishQuest -= FinishQuest;
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

    private void Update()
    {
        foreach (Quest q in questMap.Values)
        {
            if (q.state == Quest_State.REQUIREMENTS_NOT_MET && CheckRequirementsMet(q))
                ChangeQuestState(q.Info.id, Quest_State.IN_PROGRESS); // May later want to use CAN_START to not auto-start
        }
    }

    private void StartQuest(string id)
    {
        Debug.Log($"Q_MAN started quest {id}");
    }

    private void AdvanceQuest(string id)
    {
        Debug.Log($"Q_MAN advanced quest {id}");
    }

    private void FinishQuest(string id)
    {
        Debug.Log($"Q_MAN finished quest {id}");
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
    private Quest GetQuestByID(string id)
    {
        Quest quest = questMap[id];
        if (quest == null)
            Debug.LogError($"[Q_Man] Quest id {id} not found in quest map");
        return quest;
    }

}
