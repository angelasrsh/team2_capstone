using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Credit to https://www.youtube.com/watch?v=UyTJLDGcT64

/// <summary>
/// Holds data relevant to a Quest.
/// Instances of this are used to make quests (ex: tutorials).
/// </summary>
[CreateAssetMenu(fileName = "QuestInfoSO", menuName = "Quest/QuestInfo")]
public class Quest_Info_SO : ScriptableObject
{
    // Unique name for identifying quest
    [field: SerializeField] public string id { get; private set; }

    // Display name
    public string DisplayName;

    // Set this if the quest is only active in certain rooms
    public List<Room_Data.RoomID> InactiveRooms;

    // Quest prerequisites (ex: finishing other quests)
    public Quest_Info_SO[] QuestPrerequisites;

    // Quest step GameObjects holding the quest step info
    public GameObject[] QuestStepPrefabs;

    // List of strings for instructions, dialogue, etc.
    //public String[] dialogueList; Replaced with individual-step dialogue

    // Take identifier name from filename
    private void OnValidate()
    {
#if UNITY_EDITOR
        id = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

}
