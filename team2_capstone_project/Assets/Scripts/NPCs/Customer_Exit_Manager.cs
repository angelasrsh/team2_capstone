using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer_Exit_Manager : MonoBehaviour
{
    public static Customer_Exit_Manager Instance;

    [SerializeField] private List<Transform> exits = new List<Transform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Transform GetRandomExit()
    {
        if (exits.Count == 0) return null;
        return exits[Random.Range(0, exits.Count)];
    }
}
