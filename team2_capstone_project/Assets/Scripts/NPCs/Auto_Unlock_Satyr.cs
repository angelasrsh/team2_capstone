using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grimoire;

public class Auto_Unlock_Satyr : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CustomerData satyrData;
    [SerializeField] private int unlockDay = 3;

    private Customer_Spawner customerSpawner;

    private void Awake()
    {
        customerSpawner = GetComponent<Customer_Spawner>();
        if (customerSpawner == null)
            Debug.LogWarning("[Auto_Unlock_Satyr] No Customer_Spawner found on this object.");
    }

    private void OnEnable() => Day_Turnover_Manager.OnDayStarted += HandleDayStarted;
    private void OnDisable() => Day_Turnover_Manager.OnDayStarted -= HandleDayStarted;

    private void Start()
    {
        // Check immediately since it's in the middle of the day
        TryUnlockNow();
    }

    private void HandleDayStarted()
    {
        TryUnlockNow();
    }

    private void TryUnlockNow()
    {
        if (Day_Turnover_Manager.Instance == null)
        {
            Debug.LogWarning("[Auto_Unlock_Satyr] No Day_Turnover_Manager instance found.");
            return;
        }

        int currentDay = ((int)Day_Turnover_Manager.Instance.CurrentDay) + 1;
        Debug.Log($"[Auto_Unlock_Satyr] Checking unlock condition for day {currentDay} ({Day_Turnover_Manager.Instance.CurrentDay}).");

        if (currentDay >= unlockDay)
            UnlockSatyr();
        else
            Debug.Log($"[Auto_Unlock_Satyr] Not yet — unlock day is {unlockDay}.");
    }

    private void UnlockSatyr()
    {
        if (Player_Progress.Instance == null)
        {
            Debug.LogWarning("[Auto_Unlock_Satyr] Player_Progress not found.");
            return;
        }

        // Avoid unlocking multiple times
        if (Player_Progress.Instance.IsNPCUnlocked(satyrData.npcID))
        {
            Debug.Log("[Auto_Unlock_Satyr] Satyr already unlocked, skipping.");
            return;
        }

        Player_Progress.Instance.UnlockNPC(satyrData.npcID);
        Debug.Log("[Auto_Unlock_Satyr] Satyr unlocked in player progress!");

        if (customerSpawner != null && satyrData != null)
        {
            bool alreadyInList = false;
            foreach (var c in customerSpawner.possibleCustomers)
            {
                if (c != null && c.customerName == satyrData.customerName)
                {
                    alreadyInList = true;
                    break;
                }
            }

            if (!alreadyInList)
            {
                var list = new List<CustomerData>(customerSpawner.possibleCustomers);
                list.Add(satyrData);
                customerSpawner.possibleCustomers = list.ToArray();
                Debug.Log("[Auto_Unlock_Satyr] Satyr added to Customer_Spawner permanently!");
            }
        }
        else
            Debug.LogWarning("[Auto_Unlock_Satyr] Missing SatyrData or Customer_Spawner reference.");

        Save_Manager.instance?.SaveGameData();
    }
}
