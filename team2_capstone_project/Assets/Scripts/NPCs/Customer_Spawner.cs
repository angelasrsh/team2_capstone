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
        foreach (var cState in Restaurant_State.Instance.customers)
        {
            Transform seat = Seat_Manager.Instance.GetSeatByIndex(cState.seatIndex);
            CustomerData data = FindCustomerData(cState.customerName);

            if (data == null)
            {
                Debug.LogWarning($"CustomerData for {cState.customerName} not found!");
                continue;
            }

            Customer_Controller customer = Instantiate(customerPrefab, seat.position, Quaternion.identity);
            customer.Init(data, seat, Dish_Tool_Inventory.Instance, true);

            if (cState.hasRequestedDish)
                customer.Debug_ForceRequestDish(cState.requestedDishName);

            if (data.datable)
                uniqueCustomersPresent.Add(data.customerName);

            customer.OnCustomerLeft += HandleCustomerLeft;
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
            if (data.datable && uniqueCustomersPresent.Contains(data.customerName))
                continue;

            validCustomers.Add(data);
        }

        if (validCustomers.Count == 0)
        {
            Debug.LogWarning("No valid customers to spawn.");
            return;
        }

        CustomerData chosen = validCustomers[Random.Range(0, validCustomers.Count)];

        if (chosen.datable)
            uniqueCustomersPresent.Add(chosen.customerName);

        Customer_Controller customer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        Audio_Manager.instance.PlayDoorbell();
        customer.Init(chosen, seat, Dish_Tool_Inventory.Instance);
        customer.OnCustomerLeft += HandleCustomerLeft;

        Debug.Log($"Spawned customer: {chosen.customerName}");
    }

    private void HandleCustomerLeft(string customerName)
    {
        CustomerData data = FindCustomerData(customerName);
        if (data != null && data.datable)
            uniqueCustomersPresent.Remove(customerName);
    }

    public void StartNewDay()
    {
        uniqueCustomersPresent.Clear();
    }
}
