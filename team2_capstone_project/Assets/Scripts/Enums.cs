using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// namespace Grimoire
// {
    public enum InventoryCallStatusCode // Currently not used
    {
        InventoryFull,
        NotEnoughItems,
        Success
    }

/// <summary>
/// Used to keep track of the state of quests
/// </summary>
public enum Quest_State
{
    REQUIREMENTS_NOT_MET,
    CAN_START,
    IN_PROGRESS,
    CAN_FINISH,
    FINISHED
}

    
