using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.iOS;
using UnityEngine;

// Gives the player a collection of items of a fixed size
public class Inventory : MonoBehaviour
{
    public ResourceInfo test;
    [field: SerializeField] private Dictionary<ResourceInfo, int> ResourceList { get; set; } = new Dictionary<ResourceInfo, int>();

    void Start()
    {
        //ResourceList = new Dictionary<ResourceInfo, int>;
        Debug.Log("[Invtry] test: " + test);
        AddResources(test, 1);
    }

    public void AddResources(ResourceInfo type, int count)
    {
        if (ResourceList.ContainsKey(type))
        {
            ResourceList[type] += count;
        }
        else
        {
            ResourceList.Add(type, count);
        }
        
        Debug.Log("Added " + count + " " + type.name);
    }
}
