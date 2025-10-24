using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dish_Sound_Handler : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("Dish_Sound_Handler: No AudioSource component found on this GameObject.");
        }
    }

    private void OnEnable()
    {
        Customer_Spawner.OnCustomerCountChanged += HandleCustomerCountChanged;
    }

    private void OnDisable()
    {
        Customer_Spawner.OnCustomerCountChanged -= HandleCustomerCountChanged;
    }

    private void Start()
    {
        int currentCount = FindObjectsOfType<Customer_Controller>().Length;
        HandleCustomerCountChanged(currentCount);
    }

    private void HandleCustomerCountChanged(int count)
    {
        if (count > 0)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}
