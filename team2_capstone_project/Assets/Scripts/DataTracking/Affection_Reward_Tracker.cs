using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Tracks whether the special ring reward has been unlocked/collected for a specific NPC.
/// Non-opinionated: does a simple polling check against Affection_System.GetAffectionLevel(customer)
/// </summary>
public class Affection_Reward_Tracker : MonoBehaviour
{
    public static Affection_Reward_Tracker instance;

    public CustomerData targetNPC;
    public int unlockAffectionThreshold = 75;

    [HideInInspector] public bool ringUnlocked = false;  
    [HideInInspector] public bool ringCollected = false; 
    private bool checkedOnceOnLoad = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Try restore values from save if available
        var gdata = Save_Manager.GetGameData();
        if (gdata != null)
        {
            // fields may not exist in old saves
            try
            {
                // Use reflection-less approach: Save_Manager will call Restore on load (see below)
                // So this is just a safe guard if Save_Manager didn't restore
            }
            catch { }
        }
    }

    private void Update()
    {
        if (!ringUnlocked && targetNPC != null && Affection_System.Instance != null)
        {
            int val = Affection_System.Instance.GetAffectionLevel(targetNPC);
            if (val >= unlockAffectionThreshold)
            {
                ringUnlocked = true;
                Debug.Log($"[AffectionRewardTracker] Ring unlocked for {targetNPC.customerName} at affection {val}.");
                Save_Manager.instance?.AutoSave();  // ensure event unlock is saved
            }
        }
    }

    /// <summary>
    /// Helper to reset the reward (for testing)
    /// </summary>
    public void ResetReward()
    {
        ringUnlocked = false;
        ringCollected = false;
        Save_Manager.instance?.AutoSave();
    }
}
