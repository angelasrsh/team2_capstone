using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee_Activate : MonoBehaviour
{
    [SerializeField] private Bee_Guide beeGuide;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            beeGuide.Activate();
            gameObject.SetActive(false); // Only activates once
        }
    }
}
