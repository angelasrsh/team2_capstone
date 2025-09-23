using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Credit to https://www.youtube.com/watch?v=UyTJLDGcT64

/// <summary>
/// Holds static and state information for a quest
/// By referencing the QuestInfoSO and keeping track of state
/// </summary>
public class Quest
{
    public Quest_Info_SO Info;

    public Quest_State state;

    private int currentQuestStepIndex;

    public Quest(Quest_Info_SO questInfo)
    {
        Info = questInfo;
        state = Quest_State.REQUIREMENTS_NOT_MET;
        currentQuestStepIndex = 0;
    }

    public void MoveToNextStep()
    {
        currentQuestStepIndex++;
    }

    public bool CurrentStepExists()
    {
        return (currentQuestStepIndex < Info.QuestStepPrefabs.Length);
    }

    /// <summary> 
    /// Called by Quest.cs whenever we need to start a new quest step
    /// </summary>
    /// <param name="parentTransform"> What the new prefab should attach to (XXXXXXXXXXXXXXX)</param>
    public void InstantiateCurrentQuestStep(Transform parentTransform)
    {
        GameObject questStepPrefab = GetCurrentQuestStepPrefab();
        if (questStepPrefab != null)
            Object.Instantiate<GameObject>(questStepPrefab, parentTransform);

    }

    /// <summary>
    /// Called by Quest's InstantiateCurrentQuestStep method.
    /// Find the next quest step on the list using the Quest Info SO
    /// </summary>
    /// <returns> A prefab for the next quest step</returns>
    private GameObject GetCurrentQuestStepPrefab()
    {
        if (CurrentStepExists())
            return Info.QuestStepPrefabs[currentQuestStepIndex];
        else
            return null;                
    }


}
