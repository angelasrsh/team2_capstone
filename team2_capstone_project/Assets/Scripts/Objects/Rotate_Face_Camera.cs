using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Rotate_Face_Camera : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject SpritePivot; // Parent object of the sprite
    public GameObject SpriteObject;
    public int manualShift = 0;

    void Start() {
        Camera mainCamera = Camera.main;
        SpritePivot.transform.rotation = mainCamera.transform.rotation;

        // Shift sprite to other end of so the head is closer to the collider
        Vector3 position = SpritePivot.transform.position;
        float height = SpriteObject.GetComponent<SpriteRenderer>().bounds.size.y;
        float angle = mainCamera.transform.rotation.x;
        double shiftDistance = Math.Sin(angle) * height;
        SpritePivot.transform.position -= new Vector3(0, 0, ((float)shiftDistance / 2) + manualShift); // shift by half
    }


}
