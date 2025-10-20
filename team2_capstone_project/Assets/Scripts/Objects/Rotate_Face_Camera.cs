using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate_Face_Camera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        Camera mainCamera = Camera.main;
        transform.rotation = mainCamera.transform.rotation;
    }

 
}
