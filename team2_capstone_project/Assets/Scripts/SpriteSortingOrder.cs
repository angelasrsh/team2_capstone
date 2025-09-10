using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSortingOrder : MonoBehaviour
{
    [SerializeField] private int sortingOffset = 0;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        sr.sortingOrder = (int)(-transform.position.y * 100) + sortingOffset;
    }
}