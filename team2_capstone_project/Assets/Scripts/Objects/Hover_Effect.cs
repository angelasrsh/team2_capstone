using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover_Effect : MonoBehaviour
{
    [Header("Hover Settings")]
    public float amplitude = 0.25f;  
    public float frequency = 1f;   

    private Vector3 startPos;

    void Start() 
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
