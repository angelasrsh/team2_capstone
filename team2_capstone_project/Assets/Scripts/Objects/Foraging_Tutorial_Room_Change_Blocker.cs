using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foraging_Tutorial_Room_Change_Blocker : MonoBehaviour
{
    // Assign in Inspector
    [Header("GameObject that blocks player")]
    [SerializeField] private GameObject Room_Change_Trigger_Block_Collider;
    // [SerializeField] private int UnblockExitQuestStepIndex; 
    void OnEnable()
    {
        Game_Events_Manager.Instance.onHarvestRequirementsMet += UnblockExit;

        if (Room_Change_Trigger_Block_Collider == null)
            Debug.Log("[F_Tut_R_C_Block] Error: No room change trigger wall. Please assign it in the editor");
        
    }

    void OnDisable() {
        Game_Events_Manager.Instance.onHarvestRequirementsMet -= UnblockExit;
    }

    void UnblockExit()
    {
        Room_Change_Trigger_Block_Collider.SetActive(false);
    }
}
