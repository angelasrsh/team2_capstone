using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Impact_Lines_UI : MonoBehaviour
{
    [Header("Wiggle Settings")]
    public float wiggleAmplitude = 10f;   // pixel movement range
    public float wiggleFrequency = 8f;    // wiggle speed (Hz)
    public float scaleAmplitude = 0.05f;  // % scale change
    public float fadeSpeed = 5f;
    public float maxAlpha = 0.8f;

    private Image img;
    private RectTransform rect;
    private float targetAlpha = 0f;
    private Vector2 basePos;
    private Vector3 baseScale;
    private float offset;

    private void Awake()
    {
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        basePos = rect.anchoredPosition;  
        baseScale = rect.localScale;
        offset = Random.Range(0f, 100f);
        SetAlpha(0f);
    }

    private void Update()
    {
        // Wiggle effect
        float wiggleX = Mathf.Sin(Time.time * wiggleFrequency + offset);
        float wiggleY = Mathf.Cos(Time.time * wiggleFrequency * 0.7f + offset);

        rect.anchoredPosition = basePos + new Vector2(wiggleX, wiggleY) * wiggleAmplitude;
        rect.localScale = baseScale * (1f + Mathf.Sin(Time.time * wiggleFrequency) * scaleAmplitude);

        // Fade in/out
        Color c = img.color;
        c.a = Mathf.MoveTowards(c.a, targetAlpha * maxAlpha, Time.deltaTime * fadeSpeed);
        img.color = c;
    }

    public void ShowLines() => targetAlpha = 1f;
    public void HideLines() => targetAlpha = 0f;

    private void SetAlpha(float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
