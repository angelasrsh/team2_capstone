using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the sprite images (ordered) for a dating event
/// </summary>
[CreateAssetMenu(menuName = "NPCs/Event", fileName = "Event")] 
public class Event_Data : ScriptableObject
{
    [Header("Event identifiers")]
    public CustomerData Customer;
    public int MilestonePercent;

    [Header("Panel images")]
    public Sprite[] panels;

}
