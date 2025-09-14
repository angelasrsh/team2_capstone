using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace Grimoire
{
    public class Interactable_Object : MonoBehaviour
    {

    
        public GameObject InteractIcon; // Not sure if this is really needed
        private bool playerInside = false;


        void Awake()
        {
            if (InteractIcon == null)
            {
                InteractIcon = transform.Find("Interact_Icon").gameObject;
            }
            
        } 

        void OnTriggerEnter(Collider other)
        {
            // show interact option
            if (other.gameObject.CompareTag("Player"))
            {
                InteractIcon.SetActive(true);
                playerInside = true;
            }

        }

        void Update()
        {
            if (playerInside && Input.GetKeyDown(KeyCode.E))
            {
                PerformInteract();
            }
            
        }

        void OnTriggerExit(Collider other)
        {
            // hide interact option
            if (other.gameObject.CompareTag("Player"))
            {
                InteractIcon.SetActive(false);
                playerInside = false;
            }

        }

        /// <summary>
        /// Action that happens when player presses 'E' to interact while nearby.
        /// </summary>
        public virtual void PerformInteract()
        {
            Debug.Log($"[IntObj] Player interacted with " + gameObject.ToString());
        }
    }

}
