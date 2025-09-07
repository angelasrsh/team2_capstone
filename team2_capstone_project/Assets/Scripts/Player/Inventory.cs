using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.iOS;
using UnityEngine;

// Gives the player a collection of items of a fixed size
public class Inventory : MonoBehaviour
{
    [field: SerializeField] private Dictionary<ResourceInfo, int> ResourceList { get; set; }

    public void AddResources(ResourceInfo type, int count)
    {
        ResourceList[type] += count;
    }
}
