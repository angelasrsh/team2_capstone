using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPCs/NPC Database")]
public class NPC_Database : ScriptableObject
{
    [SerializeField] public List<CustomerData> allNPCs = new List<CustomerData>();
    private Dictionary<CustomerData.NPCs, CustomerData> lookup;
    public static NPC_Database Instance;

    private void OnEnable()
    {
        Instance = this;
        lookup = new Dictionary<CustomerData.NPCs, CustomerData>();
        foreach (var npc in allNPCs)
        {
            if (!lookup.ContainsKey(npc.npcID))
                lookup.Add(npc.npcID, npc);
        }
    }

    public CustomerData GetNPCData(CustomerData.NPCs npcID)
    {
        if (lookup.TryGetValue(npcID, out var data))
            return data;

        Debug.LogWarning($"NPC with ID {npcID} not found in database!");
        return null;
    }

    public CustomerData.NPCs GetNPCEnum(CustomerData data)
    {
        return data != null ? data.npcID : CustomerData.NPCs.None;
    }
}
