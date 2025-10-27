using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
    [RequireComponent(typeof(Rigidbody))]
    public class Freeze_On_Ground : MonoBehaviour
    {
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float settleDelay = 0.1f;
        [SerializeField] private bool disableColliderAfterSettle = true;

        private Rigidbody rb;
        private bool hasLanded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Check if we hit something in the ground mask
            if (((1 << collision.gameObject.layer) & groundMask) != 0 && !hasLanded)
            {
                StartCoroutine(FreezeAfterDelay(settleDelay));
            }
        }

        private System.Collections.IEnumerator FreezeAfterDelay(float delay)
        {
            hasLanded = true;
            yield return new WaitForSeconds(delay);

            // Freeze movement and rotation completely
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            if (disableColliderAfterSettle)
            {
                // Disable the collider so player can't push it
                CapsuleCollider col = GetComponentInChildren<CapsuleCollider>();
                if (col != null)
                    col.enabled = false;
            }
        }
    }
}
