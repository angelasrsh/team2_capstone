using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        SpawnCustomers();
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

        // Pick random customer data
        CustomerData data = possibleCustomers[Random.Range(0, possibleCustomers.Length)];

        // Spawn customer prefab
        Customer_Controller customer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        customer.Init(data, seat, Dish_Tool_Inventory.Instance);
    }
}
