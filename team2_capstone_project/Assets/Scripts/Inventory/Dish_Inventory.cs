using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// You can currently store dishes indefinitely. Probably not desired behavior,
/// but I'm leaving it for now. We probably change it semi-easily later.
/// </summary>
public class Dish_Inventory : Inventory
{
    public static Dish_Inventory Instance { get; private set; }
    new private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        base.Awake();
    }
    
}
