using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AffectionItemEventEntry
{
    public CustomerData npc;
    public Ingredient_Data eventItem;
    public int affectionThreshold = 75;
    [HideInInspector] public bool unlocked;
    [HideInInspector] public bool collected;
}

public class Affection_Event_Item_Tracker : MonoBehaviour
{
    public static Affection_Event_Item_Tracker instance;

    [Header("Affection Event Items")]
    public List<AffectionItemEventEntry> items = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

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

    private void HandleAffectionChanged(CustomerData npc, int newAffection)
    {
        foreach (var entry in items)
        {
            if (!entry.unlocked &&
                entry.npc != null &&
                entry.npc.npcID == npc.npcID &&
                newAffection >= entry.affectionThreshold)
            {
                entry.unlocked = true;
                Debug.Log($"[AffectionRewardTracker] {entry.eventItem.name} unlocked for {entry.npc.customerName} (affection {newAffection}).");
                Save_Manager.instance?.AutoSave();
            }
        }
    }

    public bool TryGetUnlockedEventItem(CustomerData npc, out AffectionItemEventEntry entry)
    {
        entry = items.Find(r => 
            r.npc != null &&
            r.npc.npcID == npc.npcID &&
            r.unlocked &&
            !r.collected);
        return entry != null;
    }

    public void RecheckUnlocks()
    {
        if (Affection_System.Instance == null) return;

        foreach (var entry in items)
        {
            if (!entry.unlocked && entry.npc != null)
            {
                int affection = Affection_System.Instance.GetAffectionLevel(entry.npc);
                if (affection >= entry.affectionThreshold)
                {
                    entry.unlocked = true;
                    Debug.Log($"[AffectionTracker] {entry.eventItem.name} unlocked for {entry.npc.customerName} (affection {affection}).");
                }
            }
        }
    }

    public void ResetTracker()
    {
        foreach (var entry in items)
        {
            entry.unlocked = false;
            entry.collected = false;
        }
        Debug.Log("[AffectionTracker] Tracker reset for new save.");
    }

    public void MarkCollected(AffectionItemEventEntry entry)
    {
        entry.collected = true;
        Save_Manager.instance?.AutoSave();
    }

    public bool IsItemCollected(CustomerData npc)
    {
        var entry = items.Find(r => r.npc == npc);
        return entry != null && entry.collected;
    }

    public List<AffectionEventItemsSaveData> GetSaveData()
    {
        List<AffectionEventItemsSaveData> list = new();
        foreach (var item in items)
        {
            list.Add(new AffectionEventItemsSaveData
            {
                npcID = item.npc.npcID.ToString(),
                itemName = item.eventItem != null ? item.eventItem.Name : "",
                unlocked = item.unlocked,
                collected = item.collected
            });
        }
        return list;
    }

    public void LoadFromSaveData(List<AffectionEventItemsSaveData> data)
    {
        foreach (var saved in data)
        {
            var item = items.Find(r => r.npc != null && r.npc.npcID.ToString() == saved.npcID);
            if (item != null)
            {
                item.unlocked = saved.unlocked;
                item.collected = saved.collected;
            }
        }
    }
}
