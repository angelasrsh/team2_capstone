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

    // Avoiding saving before customers have been initialized
    public bool customersInitialized {get; private set;}

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

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => Save_Manager.HasLoadedGameData);

        if(Player_Progress.Instance.InGameplayTutorial)
        {
            // still restore existing customers
            if (Restaurant_State.Instance != null && Restaurant_State.Instance.customers.Count > 0)
                RestoreCustomersFromState();
                customersInitialized = true;

            yield break; // but DO NOT spawn normal customers
        }

        // Restore saved customers (from previous evening if returning mid-day)
        if (Restaurant_State.Instance != null && Restaurant_State.Instance.customers.Count > 0)
        {
            Debug.Log("Customer_Spawner: Restoring saved customers...");
            RestoreCustomersFromState();
        }
        else
        {
            // if (Day_Turnover_Manager.Instance != null && Day_Turnover_Manager.Instance.currentTimeOfDay != Day_Turnover_Manager.TimeOfDay.Morning)
            Debug.Log("Customer_Spawner: Spawning fresh customers for new day...");
            StartCoroutine(SpawnCustomersCoroutine());
        }
        customersInitialized = true;
        
        
    }

    private void HandleDayStarted()
    {
        Debug.Log("Customer_Spawner: New day started, clearing previous customers and spawning new ones.");

        // Clear internal tracking
        StartNewDay();

        // Ensure Restaurant_State is clean
        if (Restaurant_State.Instance != null)
            Restaurant_State.Instance.ResetRestaurantState();

        // Check if we're in the tutorial
        if (Player_Progress.Instance != null && Player_Progress.Instance.InGameplayTutorial)
        {
            Debug.Log("Tutorial active, skipping daily customer spawn.");
            return;
        }

        // // Spawn new customers for the day in the evening only
        // if (Day_Turnover_Manager.Instance.currentTimeOfDay == Day_Turnover_Manager.TimeOfDay.Evening)
            StartCoroutine(SpawnCustomersCoroutine());
    }

    private void RestoreCustomersFromState()
    {
        if (Restaurant_State.Instance == null)
        {
            Debug.LogWarning("Customer_Spawner: No Restaurant_State found during restore.");
            return;
        }

        var savedCustomers = new List<Customer_State>(Restaurant_State.Instance.customers);

        foreach (var cState in savedCustomers)
        {
            Transform seat = Seat_Manager.Instance.GetSeatByIndex(cState.seatIndex);
            if (seat == null)
            {
                Debug.LogWarning($"Seat index {cState.seatIndex} invalid for {cState.customerName}!");
                continue;
            }

            CustomerData data = FindCustomerData(cState.customerName);
            if (data == null)
            {
                Debug.LogWarning($"CustomerData for {cState.customerName} not found!");
                continue;
            }

            Customer_Controller customer = Instantiate(customerPrefab, seat.position, Quaternion.identity);
            customer.Init(data, seat, Dish_Tool_Inventory.Instance, spawnSeated: true);
            customer.RestoreFromState(cState);

            if (cState.isTutorialCustomer)
            {
                customer.isTutorialCustomer = true;
                Debug.Log("Restored tutorial customer properly.");
            }

            if (data.datable)
                uniqueCustomersPresent.Add(data.customerName);

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

    /// <summary>
    /// Spawn a customer using the given CustomerData
    /// </summary>
    /// <param name="chosen"></param>
    public void SpawnSingleCustomer(CustomerData chosen)
    {
        if (chosen == null)
            Helpers.printLabeled(this, "Please assign a customer before spawning it");

        Transform seat = Seat_Manager.Instance.GetRandomAvailableSeat();
        if (seat == null)
        {
            Debug.Log("No free seats!");
            return;
        }

        if (chosen.datable)
            uniqueCustomersPresent.Add(chosen.customerName);

        Customer_Controller customer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        Audio_Manager.instance.PlayDoorbell();
        customer.Init(chosen, seat, Dish_Tool_Inventory.Instance);
        customer.OnCustomerLeft += HandleCustomerLeft;
        OnCustomerCountChanged?.Invoke(GetCurrentCustomerCount());

        Debug.Log($"Spawned customer: {chosen.customerName}");
    }

    /// <summary>
    /// Spawns a customer and returns the Customer_Controller for cases like the tutorial.
    /// </summary>
    public Customer_Controller SpawnSingleCustomerReturn(CustomerData chosen)
    {
        if (chosen == null)
        {
            Helpers.printLabeled(this, "Please assign a customer before spawning it");
            return null;
        }

        Transform seat = Seat_Manager.Instance.GetRandomAvailableSeat();
        if (seat == null)
        {
            Debug.Log("No free seats!");
            return null;
        }

        if (chosen.datable)
            uniqueCustomersPresent.Add(chosen.customerName);

        Customer_Controller customer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        Audio_Manager.instance?.PlayDoorbell();

        customer.Init(chosen, seat, Dish_Tool_Inventory.Instance);
        customer.OnCustomerLeft += HandleCustomerLeft;

        OnCustomerCountChanged?.Invoke(GetCurrentCustomerCount());

        Debug.Log($"Spawned customer (RETURNING): {chosen.customerName}");
        return customer;
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
