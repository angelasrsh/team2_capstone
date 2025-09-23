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

public enum IngredientType // should delete after putting in maddie's code
{
    Null,
    Egg,
    Melon,
    Morel,
    Milk,
    Cheese,
    Uncut_Fogshroom,
    Uncut_Fermented_Eye,
    Slime,
    Bone_Broth,
    Bone,
    Cut_Fermented_Eye,
    Cut_Fogshroom
}

/// <summary>
/// Used to keep track of the state of quests
/// </summary>
public enum Quest_State
{
    REQUIREMENTS_NOT_MET,
    //CAN_START,
    IN_PROGRESS,
    //CAN_FINISH,
    FINISHED
}

    
