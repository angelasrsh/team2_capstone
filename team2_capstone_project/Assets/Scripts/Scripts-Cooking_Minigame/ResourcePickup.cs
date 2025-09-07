using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;

// Script for an ingredient that can be picked up
public class ResourcePickup : InteractableObject
{
    [field: SerializeField] public ResourceInfo ResourceType { get; private set; }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
