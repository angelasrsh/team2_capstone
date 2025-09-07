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

    // Update is called once per frame
    void Update()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(horizontalMovement * moveSpeed, rb.velocity.y, verticalMovement * moveSpeed);
        //Debug.Log("hi?");

    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        verticalMovement = context.ReadValue<Vector2>().y;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player calls OnTriggerEnter");
    }

      void OnTriggerExit(Collider other)
    {
        Debug.Log("Player calls OnTriggerExit");
    }
}
