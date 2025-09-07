using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Grimoire
{
    public class InteractableObject : MonoBehaviour
{
    private bool isInRange; // Not sure if this is needed
    public GameObject InteractIcon; // Not sure if this is really needed

    void OnTriggerEnter(Collider other)
    {
        // show interact option
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = true;
            InteractIcon.SetActive(true);
            Debug.Log("Set interact icon active");

        }
        Debug.Log("TriggerEnter" + other.gameObject.name);

    }

    void OnTriggerExit(Collider other)
    {
        // hide interact option
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = false;
            InteractIcon.SetActive(false);
            Debug.Log("Set interact icon inactive");
            
            
        }
    Debug.Log("TriggerExit " + other.gameObject.name);

    }
}

}
