using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void Start()
    {
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
            }
        }
        else
        {
            SpawnCustomers();
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

    public void SpawnCustomers()
    {
        int customerCount = Random.Range(minCustomers, maxCustomers);
        StartCoroutine(SpawnSequence(customerCount));
        Debug.Log("Started spawning customers...");
    }

    private IEnumerator SpawnSequence(int count)
    {
        Debug.Log($"Spawning {count} customers...");
        for (int i = 0; i < count; i++)
        {
            SpawnCustomer();
            yield return new WaitForSeconds(Random.Range(minSpawnerWaitTime, maxSpawnerWaitTime));
        }
    }

    private void SpawnCustomer()
    {
        Transform seat = Seat_Manager.Instance.GetRandomAvailableSeat();
        if (seat == null)
        {
            Debug.Log("No free seats!");
            return;
        }

        CustomerData data = possibleCustomers[Random.Range(0, possibleCustomers.Length)];

        Customer_Controller customer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        customer.Init(data, seat, Dish_Tool_Inventory.Instance);
    }
}
