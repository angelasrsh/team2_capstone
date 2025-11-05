using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the sprite images (ordered) for a dating event
/// </summary>
[CreateAssetMenu(menuName = "NPCs/Event", fileName = "Event")] 
public class Event_Data : ScriptableObject
{
    public Room_Data.RoomID roomToReturnTo;

    [Header("Event Identifiers")]
    public string CutsceneID;  // e.g. "Elf_50", "Satyr_100", etc.
    public CustomerData Customer;
    public int MilestonePercent;

    [Header("Panel Images")]
    public Sprite[] Panels;

    [Header("Music")]
    public AudioClip Music;
}
