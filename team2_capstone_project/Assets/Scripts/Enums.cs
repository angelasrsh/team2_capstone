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

public class Helpers
{
    /// <summary>
    /// Helper function to print the string with the script and gameObject name. String must be quote-enclosed.
    /// </summary>
    /// <param name="script"> the script calling this function </param>
    /// <param name="str"> The quote-enclosed string to print </param>
    public static void printLabeled(MonoBehaviour script, string str)
    {
        Debug.Log($"{script.GetType().Name} on {script.name}: " + str);
    }
}

    
