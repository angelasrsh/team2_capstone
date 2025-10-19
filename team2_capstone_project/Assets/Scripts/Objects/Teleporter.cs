using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportDestination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                // Temporarily disable CharacterController to avoid collision issues
                controller.enabled = false;

                other.transform.position = teleportDestination.position;
                Audio_Manager.instance?.PlaySFX(Audio_Manager.instance.teleport, 0.7f, 1.25f);

                controller.enabled = true;
            }
            else
                other.transform.position = teleportDestination.position;
        }
    }
}
