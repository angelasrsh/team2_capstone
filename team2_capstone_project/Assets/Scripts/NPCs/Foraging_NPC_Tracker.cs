using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foraging_NPC_Tracker : MonoBehaviour
{
    private Foraging_NPC_Spawner spawner;
    private string npcID;

    public void Init(Foraging_NPC_Spawner s, string id)
    {
        spawner = s;
        npcID = id;
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.UnregisterNPC(npcID);
    }
}