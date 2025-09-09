using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeCustomerSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public int minCustomers = 1;
    public int maxCustomers = 3;
    public float minSpawnerWaitTime = 2f;
    public float maxSpawnerWaitTime = 5f;
    public CustomerData[] possibleCustomers;

    [Header("References")]
    public Transform entrancePoint;
    public CafeCustomerController customerPrefab;

    public void Awake() {}

    public void SpawnCustomers()
    {
        int customerCount = Random.Range(minCustomers, maxCustomers);
        StartCoroutine(SpawnSequence(customerCount));
    }

    private IEnumerator SpawnSequence(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnCustomer();
            yield return new WaitForSeconds(Random.Range(minSpawnerWaitTime, maxSpawnerWaitTime));
        }
    }

    private void SpawnCustomer()
    {
        Transform seat = CafeSeatManager.Instance.GetRandomAvailableSeat();
        if (seat == null)
        {
            Debug.Log("No free seats!");
            return;
        }

        // Pick random customer data
        CustomerData data = possibleCustomers[Random.Range(0, possibleCustomers.Length)];

        // Spawn NPC prefab
        CafeCustomerController customer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        customer.Init(data, seat);
    }
}
