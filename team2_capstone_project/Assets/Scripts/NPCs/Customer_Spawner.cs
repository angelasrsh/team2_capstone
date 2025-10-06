using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Grimoire;

public class Customer_Spawner : MonoBehaviour
{
    [SerializeField] private Inventory inventoryManager;

    [Header("Spawner Settings")]
    public int minCustomers = 1;
    public int maxCustomers = 3;
    public float minSpawnerWaitTime = 2f;
    public float maxSpawnerWaitTime = 5f;
    public CustomerData[] possibleCustomers;

    [Header("References")]
    public Transform entrancePoint;
    public Customer_Controller customerPrefab;

    // Track which customers are currently present this evening
    private HashSet<string> uniqueCustomersPresent = new HashSet<string>();
    List<CustomerData> validCustomers = new List<CustomerData>();

       public void Start()
    {
        // Restore saved customers
        if (Restaurant_State.Instance.customers.Count > 0)
        {
            foreach (var cState in Restaurant_State.Instance.customers)
            {
                Transform seat = Seat_Manager.Instance.GetSeatByIndex(cState.seatIndex);
                CustomerData data = FindCustomerData(cState.customerName);

                // Spawn directly at seat when restoring
                Customer_Controller customer = Instantiate(customerPrefab, seat.position, Quaternion.identity);
                customer.Init(data, seat, Dish_Tool_Inventory.Instance, true);

                if (cState.hasRequestedDish)
                {
                    customer.Debug_ForceRequestDish(cState.requestedDishName);
                }

                // Only track unique NPCs in the HashSet
                if (data.datable)
                {
                    uniqueCustomersPresent.Add(data.customerName);
                }

                customer.OnCustomerLeft += HandleCustomerLeft;
            }
        }
        else
        {
            // Start coroutine to spawn multiple customers for a new day
            StartCoroutine(SpawnCustomersCoroutine());
        }
    }

   private CustomerData FindCustomerData(string name)
    {
        foreach (var data in possibleCustomers)
        {
            if (data.customerName == name)
                return data;
        }
        Debug.LogWarning($"CustomerData for {name} not found in possibleCustomers!");
        return null;
    }

    /// <summary>
    /// Spawns between minCustomers and maxCustomers customers with delays.
    /// </summary>
    private IEnumerator SpawnCustomersCoroutine()
    {
        int customerCount = Day_Plan_Manager.instance != null 
            ? Day_Plan_Manager.instance.customersPlannedForEvening 
            : Random.Range(minCustomers, maxCustomers + 1);
            
        Debug.Log($"Spawning {customerCount} customers based on daily plan...");

        for (int i = 0; i < customerCount; i++)
        {
            SpawnSingleCustomer();

            // wait between spawns so they don’t all appear at once
            float wait = Random.Range(minSpawnerWaitTime, maxSpawnerWaitTime);
            yield return new WaitForSeconds(wait);
        }
    }

     /// <summary>
    /// Spawns one customer at a random available seat from the filtered list.
    /// </summary>
    private void SpawnSingleCustomer()
    {
        Transform seat = Seat_Manager.Instance.GetRandomAvailableSeat();
        if (seat == null)
        {
            Debug.Log("No free seats!");
            return;
        }

        // Filter possible customers
        List<CustomerData> validCustomers = new List<CustomerData>();
        foreach (var data in possibleCustomers)
        {
            // skip only if UNIQUE NPC already present
            if (data.datable && uniqueCustomersPresent.Contains(data.customerName))
                continue;

            validCustomers.Add(data);
        }

        if (validCustomers.Count == 0)
        {
            Debug.LogWarning("No valid customers to spawn.");
            return;
        }

        // Pick one
        CustomerData chosen = validCustomers[Random.Range(0, validCustomers.Count)];

        // Track only if UNIQUE
        if (chosen.datable)
        {
            uniqueCustomersPresent.Add(chosen.customerName);
        }

        // Spawn at entrance and walk to seat
        Customer_Controller customer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        Audio_Manager.instance.PlayDoorbell();
        customer.Init(chosen, seat, Dish_Tool_Inventory.Instance);

        // Let spawner know when this customer leaves
        customer.OnCustomerLeft += HandleCustomerLeft;

        Debug.Log($"Spawned customer: {chosen.customerName}");
    }

    private void HandleCustomerLeft(string customerName)
    {
        // Find data for the customer name
        CustomerData data = FindCustomerData(customerName);
        if (data != null && data.datable)
        {
            uniqueCustomersPresent.Remove(customerName);
        }
    }

    /// <summary>
    /// Call this when a new day/evening starts to clear the presence list.
    /// </summary>
    public void StartNewDay()
    {
        uniqueCustomersPresent.Clear();
    }
}
