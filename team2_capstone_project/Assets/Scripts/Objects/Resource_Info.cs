using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resources", menuName = "FabledFeast/Resource")]
public class Resource_Info : ScriptableObject
{

    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public float DropRate { get; private set; }
}
