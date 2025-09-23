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
public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }
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
    public void PlayerMoved()
    {
        if (onPlayerMove != null) // Guessing null means no subscribers?
            onPlayerMove();
        Debug.Log("[G_E_M] Player Moved");
    }
}
