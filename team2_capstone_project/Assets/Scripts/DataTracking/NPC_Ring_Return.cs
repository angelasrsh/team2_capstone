using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Ring_Return : MonoBehaviour
{
    [Header("Ring Reward Settings")]
    public CustomerData targetNPC;             
    public Ingredient_Data ringItem;           
    public int affectionThreshold = 75;    
    private bool hasGivenRing = false;         

    private void OnEnable()
    {
        if (Affection_System.Instance != null)
            Affection_System.Instance.OnAffectionChanged += HandleAffectionChanged;
    }

    private void OnDisable()
    {
        if (Affection_System.Instance != null)
            Affection_System.Instance.OnAffectionChanged -= HandleAffectionChanged;
    }

    private void HandleAffectionChanged(CustomerData data, int affection)
    {
        if (hasGivenRing) return; // already rewarded
        if (data == null || targetNPC == null) return;
        if (data.npcID != targetNPC.npcID) return; // only react to this specific NPC

        if (affection >= affectionThreshold)
        {
            GiveRingReward();
            hasGivenRing = true;
        }
    }

    private void GiveRingReward()
    {
        Debug.Log($"{targetNPC.customerName} returns the ring!");

        Inventory playerInventory = FindObjectOfType<Inventory>();
        if (playerInventory != null && ringItem != null)
        {
            playerInventory.AddResources(ringItem, 1);
            Debug.Log($"Ring '{ringItem.name}' added to inventory.");
        }

        Dialogue_Manager dm = FindObjectOfType<Dialogue_Manager>();
        if (dm != null)
            dm.PlayScene($"{targetNPC.npcID}.RingReturn", CustomerData.EmotionPortrait.Emotion.Happy);
    }
}
