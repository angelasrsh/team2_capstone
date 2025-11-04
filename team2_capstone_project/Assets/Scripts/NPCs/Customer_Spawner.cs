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
    private List<CustomerData> validCustomers = new List<CustomerData>();
    public static event System.Action<int> OnCustomerCountChanged;

    private void OnEnable()
    {
        Day_Turnover_Manager.OnDayStarted += HandleDayStarted;
    }

    private void OnDisable()
    {
        Day_Turnover_Manager.OnDayStarted -= HandleDayStarted;
    }

    private void Start()
    {
        // Restore saved customers (from previous evening if returning mid-day)
        if (Restaurant_State.Instance != null && Restaurant_State.Instance.customers.Count > 0)
        {
            Debug.Log("Customer_Spawner: Restoring saved customers...");
            RestoreCustomersFromState();
        }
        else
        {
            Debug.Log("Customer_Spawner: Spawning fresh customers for new day...");
            StartCoroutine(SpawnCustomersCoroutine());
        }
    }

    private void HandleDayStarted()
    {
        Debug.Log("Customer_Spawner: New day started, clearing previous customers and spawning new ones.");

        // Clear internal tracking
        StartNewDay();

        // Ensure Restaurant_State is clean
        if (Restaurant_State.Instance != null)
            Restaurant_State.Instance.ResetRestaurantState();

        // Spawn new customers for the day
        StartCoroutine(SpawnCustomersCoroutine());
    }

    private void RestoreCustomersFromState()
    {
        if (Restaurant_State.Instance == null)
        {
            Debug.LogWarning("Customer_Spawner: No Restaurant_State found during restore.");
            return;
        }

        if (Restaurant_State.Instance.customers == null || Restaurant_State.Instance.customers.Count == 0)
        {
            Debug.Log("Customer_Spawner: No customers to restore.");
            return;
        }

        var savedCustomers = Restaurant_State.Instance.customers;
        foreach (var cState in savedCustomers)
        {
            // 1. Find seat
            Transform seat = Seat_Manager.Instance.GetSeatByIndex(cState.seatIndex);
            if (seat == null)
            {
                Debug.LogWarning($"Seat index {cState.seatIndex} invalid for {cState.customerName}!");
                continue;
            }

            // 2. Get customer data
            CustomerData data = FindCustomerData(cState.customerName);
            if (data == null)
            {
                Debug.LogWarning($"CustomerData for {cState.customerName} not found!");
                continue;
            }

            // 3. Spawn the customer already seated
            Customer_Controller customer = Instantiate(customerPrefab, seat.position, Quaternion.identity);
            customer.Init(data, seat, Dish_Tool_Inventory.Instance, spawnSeated: true);

            // 4. Restore requested dish if applicable
            if (cState.hasRequestedDish && !string.IsNullOrEmpty(cState.requestedDishName))
            {
                customer.Debug_ForceRequestDish(cState.requestedDishName);
                Debug.Log($"Restored {cState.customerName}'s requested dish: {cState.requestedDishName}");
            }

            // 5. Optional: Handle served customers (if you ever keep them around)
            if (cState.hasBeenServed)
            {
                // e.g. you could make them leave instantly or hide them
                customer.LeaveRestaurant();
            }

            // 6. Track datable customers
            if (data.datable)
                uniqueCustomersPresent.Add(data.customerName);

            // 7. Subscribe to events
            customer.OnCustomerLeft += HandleCustomerLeft;
        }

        Debug.Log($"Customer_Spawner: Restored {Restaurant_State.Instance.customers.Count} customers from state.");
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

    private IEnumerator SpawnCustomersCoroutine()
    {
        int customerCount = Day_Plan_Manager.instance != null
            ? Day_Plan_Manager.instance.customersPlannedForEvening
            : Random.Range(minCustomers, maxCustomers + 1);

        Debug.Log($"Customer_Spawner: Spawning {customerCount} customers for the new day...");

        for (int i = 0; i < customerCount; i++)
        {
            SpawnSingleCustomer();
            yield return new WaitForSeconds(Random.Range(minSpawnerWaitTime, maxSpawnerWaitTime));
        }
    }

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
            if (data == null)
                continue;

            // Skip if NPC isn't unlocked yet
            if (Player_Progress.Instance != null && !Player_Progress.Instance.IsNPCUnlocked(data.npcID))
            {
                continue;
            }

            if (data.datable && uniqueCustomersPresent.Contains(data.customerName))
                continue;

            validCustomers.Add(data);
        }

        if (validCustomers.Count == 0)
        {
            Debug.LogWarning("No valid customers to spawn (possibly all locked or already present).");
            return;
        }

        CustomerData chosen = validCustomers[Random.Range(0, validCustomers.Count)];

        if (chosen.datable)
            uniqueCustomersPresent.Add(chosen.customerName);

        Customer_Controller customer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        Audio_Manager.instance.PlayDoorbell();
        customer.Init(chosen, seat, Dish_Tool_Inventory.Instance);
        customer.OnCustomerLeft += HandleCustomerLeft;
        OnCustomerCountChanged?.Invoke(GetCurrentCustomerCount());

        Debug.Log($"Spawned customer: {chosen.customerName}");
    }

    private void HandleCustomerLeft(string customerName)
    {
        CustomerData data = FindCustomerData(customerName);
        if (data != null && data.datable)
            uniqueCustomersPresent.Remove(customerName);

        OnCustomerCountChanged?.Invoke(GetCurrentCustomerCount());
    }

    // Helper function
    private int GetCurrentCustomerCount()
    {
        return FindObjectsOfType<Customer_Controller>().Length;
    }

    public void StartNewDay() => uniqueCustomersPresent.Clear();
}
