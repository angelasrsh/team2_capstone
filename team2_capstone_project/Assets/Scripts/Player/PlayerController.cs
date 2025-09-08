using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;

    // Movement
    public float moveSpeed = 5f;
    float horizontalMovement;
    float verticalMovement;

    // Collision Interaction
    private GameObject itemInRange;

  // Update is called once per frame
  void Update()
  {
    rb = GetComponent<Rigidbody>();
    rb.velocity = new Vector3(horizontalMovement * moveSpeed, rb.velocity.y, verticalMovement * moveSpeed);
        
        if(itemInRange != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player picked up Ingredient: " + itemInRange.name);
            Destroy(itemInRange);
            itemInRange = null;
        }

    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        verticalMovement = context.ReadValue<Vector2>().y;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player calls OnTriggerEnter");
        if(other.CompareTag("Ingredient"))
        {
            Debug.Log("Player collided with Ingredient");
            itemInRange = other.gameObject;
        }
    }

      void OnTriggerExit(Collider other)
    {
        Debug.Log("Player calls OnTriggerExit");
        if(other.CompareTag("Ingredient"))
        {
            Debug.Log("Player exited collision with Ingredient");
            itemInRange = null;
        }
    }
}
