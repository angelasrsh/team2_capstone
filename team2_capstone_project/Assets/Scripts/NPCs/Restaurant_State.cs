using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restaurant_State : MonoBehaviour
{
    public static Restaurant_State Instance;
    public List<Customer_State> customers = new List<Customer_State>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SaveCustomers()
    {
        customers.Clear();

        // include inactive objects just in case
        Customer_Controller[] allCustomers = FindObjectsOfType<Customer_Controller>(true);
        Debug.Log($"Restaurant_State: Found {allCustomers.Length} Customer_Controller instances before saving.");

        foreach (var customer in allCustomers)
        {
            Customer_State state = customer.GetState();

            // Skip saving customers who have been served
            if (state.hasBeenServed)
            {
                Debug.Log($"Skipping {state.customerName} (already served).");
                continue;
            }

            customers.Add(state);
        }

        Debug.Log($"Restaurant_State: Saved {customers.Count} customers.");
    }
}
