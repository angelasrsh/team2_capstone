using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient_Inventory : Inventory
{
    public static Ingredient_Inventory Instance { get; private set;}

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
