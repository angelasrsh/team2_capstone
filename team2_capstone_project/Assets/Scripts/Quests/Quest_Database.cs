using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An instance of this exists in the data folder to hold all the Quest_Info_SOs.
/// Used by the Quest Manager.
/// </summary>
[CreateAssetMenu(fileName = "Quest_Database", menuName = "Quest/Quest_Database")]
public class Quest_Database : ScriptableObject
{
    public Quest_Info_SO[] allQuests;
}
