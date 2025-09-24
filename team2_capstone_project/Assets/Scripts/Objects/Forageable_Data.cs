using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this to provide data to forageable objects (ex: how many resources to spawn when harvested)
/// </summary>
[CreateAssetMenu(menuName = "Foraging/Forageable", fileName = "Forageable_Data")]
public class Forageable_Data : ScriptableObject
{
    public int ResourcesToGive;

}
