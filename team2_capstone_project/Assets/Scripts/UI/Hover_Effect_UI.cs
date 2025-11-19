using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover_Effect_UI : MonoBehaviour
{
    public float amplitude = 20f;
    public float frequency = 2f;

    private RectTransform rect;
    private Vector2 startPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        rect.anchoredPosition = new Vector2(startPos.x, newY);
    }
}
