using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

/// <summary>
/// Stores affection data for a single customer, including their current level
/// and which cutscenes have been triggered
/// TODO: Switch to using enum
/// </summary>
public class AffectionEntry {
    public const int MaxAffection = 100; // TODO: Probably should add this somewhere else to avoid repeated data
    public const int NumEvents = 4;
    public const int MinAffection = 0; // Update if we want to stop player from dropping below a certain level
    public const int AffectionMilestone = MaxAffection / NumEvents; // 25
    public CustomerData customerData;
    public int AffectionLevel;
    public bool[] EventsPlayed = new bool[NumEvents]; // default false

    // public Scene eventScene; // Temp: Delete later

   


    /// <summary>
    /// Call this function to check if there is an event that can be played
    /// And play it if able
    /// </summary>
    public void TryPlayEvent()
    {
        int milestone = NextEligibleEvent(); // 0 - 3 for 25 - 100 level events
        switch (milestone)
        {
            case 1:
                Affection_System.Instance.Cutscene = customerData.Cutscene_50;
                PlayDatingEvent();
                break;
            case 3:
                Affection_System.Instance.Cutscene = customerData.Cutscene_100;
                PlayDatingEvent();
                break;
            case 0:
            case 2:
            // TODO: Add dialogue dating events
            default:
                break;
                // Can also make a function that takes care of calling both types of events
        }    
        if (milestone > -1)
            EventsPlayed[milestone] = true; // Mark event as played        
    }

    /// <summary>
    /// Check if there is an eligible event that has yet to be played
    /// </summary>
    /// <returns> The index of the next event that can be played. -1 if none can be played </returns>
    private int NextEligibleEvent()
    {
        int maxMilestone = (AffectionLevel / AffectionMilestone);

        for (int i = 0; i < maxMilestone; i++)
        {
            if (!EventsPlayed[i])
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Play the event associated with the next eligible event
    /// </summary>
    private void PlayDatingEvent()
    {
        Debug.Log("[Aff_Sys] Date start!");

        // save restaurant state first
        if (Restaurant_State.Instance != null)
        {
            Restaurant_State.Instance.SaveCustomers();
            Debug.Log("[Aff_Sys] Restaurant state saved before cutscene.");
        }
        else
            Debug.LogWarning("[Aff_Sys] Restaurant_State instance not found before cutscene.");

        if (Save_Manager.instance != null)
            Save_Manager.instance.AutoSave();

        // then transition
        Game_Events_Manager.Instance.StartCoroutine(TransitionToDateScene());   

    }

    private IEnumerator TransitionToDateScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Dating_Events", LoadSceneMode.Single);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
            yield return null;

        Debug.Log("[Aff_Sys] Date scene loaded.");
    }

    /// <summary>
    /// Change the affection level
    /// </summary>
    /// <param name="amount"> Amount to add (cannot be negative) </param>
    public void AddAffection(int amount)
    {
        AffectionLevel += amount;

        // Clamp affection between 0 and 100
        AffectionLevel = Mathf.Clamp(AffectionLevel, MinAffection, MaxAffection);
    }
}

/// <summary>
/// Contains a function for adding affection and triggering a date event (if desired and able)
/// This singleton script currently sits on the GameManager but maybe should go elsewhere.
/// </summary>
public class Affection_System : MonoBehaviour
{
    // Store affection data for customers
    private List<AffectionEntry> CustomerAffectionEntries = new List<AffectionEntry>(); // Change to using enum
    public event Action<CustomerData, int> OnAffectionChanged;

    // Constants
    [Header("Constants")]
    [SerializeField] private int LikedDishAffection = 5; // NOTE TODO: Could maybe make the whole Liked/Disliked thing an enum or scale/int later
    [SerializeField] private int NeutralDishAffection = 2;
    [SerializeField] private int DislikedDishAffection = -5;

    public static Affection_System Instance;

    public Event_Data Cutscene; // Played by Panel_Cutscene

    // Temp variables until a better system is made
    private CustomerData nextCutsceneCustomer;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
      Game_Events_Manager.Instance.onEndDialogBox += TryPlayNextEvent;
    }

    private void OnDisable()
    {
      Game_Events_Manager.Instance.onEndDialogBox -= TryPlayNextEvent;
    }


    // Note: whatever database used should only hold dateable NPCs
    /// <summary>
    /// Check if the NPC is in the list of AffectionEntries. If they are, add an amount of affection
    /// dictated by the LikedDish, DislikedDish, or NeutralDish values.
    ///  Example keys produced: "Elf.LikedDish", "Satyr.DislikedDish", "Phrog.NeutralDish"
    /// < GenerateDialogueKey(Dish_Data servedDish)
    /// </summary>
    /// <param name="customer"> Customer data to update </param>
    /// <param name="suffix"> Suffix of LikedDish, DislikedDish, or NeutralDish generated by GenerateDialogueKey </param> 
    /// <param name="tryPlayEvent"> Trigger an event if one is ready </param> 
    public void AddAffection(CustomerData customer, string suffix, bool tryPlayEvent)
    {
        // Don't care about storing non-dateable customers
        if (!customer.datable)
            return;

        // Retrieve entry
        AffectionEntry entryToUpdate = CustomerAffectionEntries.Find(x => x.customerData == customer);

        // If null, create new entry
        if (entryToUpdate == null)
        {
            entryToUpdate = new AffectionEntry { customerData = customer };
            CustomerAffectionEntries.Add(entryToUpdate);
        }
        Debug.Log($"[AFF_SYS] updating affection for {entryToUpdate.customerData.customerName}");

        // Would probably be better to make an enum or array
        if (suffix.Equals("LikedDish"))
            entryToUpdate.AddAffection(LikedDishAffection);
        else if (suffix.Equals("NeutralDish"))
            entryToUpdate.AddAffection(NeutralDishAffection);
        else if (suffix.Equals("DislikedDish"))
            entryToUpdate.AddAffection(DislikedDishAffection);
        else
            Debug.Log($"[AFF_SYS] Error: Unknown suffix {suffix}");

        if (tryPlayEvent)
            entryToUpdate.TryPlayEvent();
        else // save CustomerData for later calls to TryPlayNextEvent
            nextCutsceneCustomer = customer;


        return;
    }

    /// <summary>
    /// If a customerdata is saved in nextCutsceneCustomer, see if you can play an event for that customer
    /// </summary>
    public void TryPlayNextEvent(string dialogKey)
    {
        // Nothing saved
        if (nextCutsceneCustomer == null)
            return;

        // Retrieve entry
        AffectionEntry entryToUpdate = CustomerAffectionEntries.Find(x => x.customerData == nextCutsceneCustomer);

        if (entryToUpdate == null)
            return; // Don't do anything if there is no entry for the customer

        // Attempt to play event
        entryToUpdate.TryPlayEvent();

    }

    /// <summary>
    /// This should be called after a dating event to clear what customer has a cutscene to be played.
    /// I don't think clearing the data is actually necessary, though.
    /// </summary>
    public void ClearNextCutsceneCustomer()
    {
        nextCutsceneCustomer = null;
    }

    public int GetAffectionLevel(CustomerData customer)
    {
        AffectionEntry entryToUpdate = CustomerAffectionEntries.Find(x => x.customerData == customer);

        // If null, create new entry
        if (entryToUpdate == null)
            return 0;
        else
            return entryToUpdate.AffectionLevel;
    }

}
