using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace Grimoire
{
    public class InteractableObject : MonoBehaviour
    {
        public GameObject InteractIcon; // Not sure if this is really needed

        void OnTriggerEnter(Collider other)
        {
            // show interact option
            if (other.gameObject.CompareTag("Player"))
            {
                InteractIcon.SetActive(true);
            }
            //Debug.Log("[" + gameObject + "] Called OnTriggerEnter");

        }

        void OnTriggerExit(Collider other)
        {
            // hide interact option
            if (other.gameObject.CompareTag("Player"))
            {
                InteractIcon.SetActive(false);
            }
            //Debug.Log("[" + gameObject + "] Called OnTriggerExit");

        }

        public void PerformInteract()
        {
            Debug.Log("[IntObj] Interaction performed");
        }
    }

}
