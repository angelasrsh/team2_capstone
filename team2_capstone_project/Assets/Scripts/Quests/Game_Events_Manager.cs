using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This singleton script attaches to the QuestManager to listen for changes in quest state
/// And broadcast when those changes occur.
/// 
/// This script must run first to initialize quests (set in script execution order)
/// </summary>
public class Game_Events_Manager : MonoBehaviour
{
    public static Game_Events_Manager Instance { get; private set; }
    private void Awake()
    {
        // Singleton
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

    // Tell objects when a quest state changes (and which quest, by ID)

    /////////// PLAYER INPUT EVENTS ////////////

    public event Action onPlayerMove;
    /// <summary>
    /// Broadcast the onPlayerMove event
    /// </summary>
    public void PlayerMoved()
    {
        if (onPlayerMove != null) // Guessing null means no subscribers?
            onPlayerMove();
        Debug.Log("[G_E_M] Player Moved");
    }

    //public event Action onP

    //////////// QUEST EVENTS /////////////
    public event Action<string> onStartQuest;
    /// <summary>
    /// Broadcast the start quest event
    /// </summary>
    /// <param name="id"> Name of the quest </param>
    public void StartQuest(string id)
    {
        if (onStartQuest != null)
            onStartQuest(id);
    }

    public event Action<string> onAdvanceQuest;
    /// <summary>
    /// Broadcast the a quest changing to the next step
    /// </summary>
    /// <param name="id"> Name of the quest </param>
    public void AdvanceQuest(string id)
    {
        if (onAdvanceQuest != null)
            onAdvanceQuest(id);
    }

    public event Action<string> onFinishQuest;
    /// <summary>
    /// Broadcast the finish quest event
    /// </summary>
    /// <param name="id"> Name of the quest </param>
    public void FinishQuest(string id)
    {
        if (onFinishQuest != null)
            onFinishQuest(id);
    }

    public event Action<Quest> onQuestStateChange;
    /// <summary>
    /// Broadcast that a quest's state has changed so listening objects know
    /// </summary>
    /// <param name="quest"> The quest whose state changed </param>
    public void QuestStateChange(Quest quest)
    {
        if (onQuestStateChange != null)
            onQuestStateChange(quest);
    }
    
}
