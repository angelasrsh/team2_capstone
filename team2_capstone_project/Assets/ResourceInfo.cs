using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resources", menuName = "FabledFeast/Resource")]
public class ResourceInfo : ScriptableObject
{

    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public float DropRate { get; private set; }
}
