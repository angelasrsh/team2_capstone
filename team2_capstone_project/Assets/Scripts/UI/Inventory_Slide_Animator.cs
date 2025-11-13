using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Slide_Animator : MonoBehaviour
{
 public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down,
        Custom
    }

    [Header("Animation Settings")]
    public float slideDuration = 0.4f;
    public SlideDirection direction = SlideDirection.Left;
    public Vector2 customOffset = new Vector2(-800f, 0f);
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Vector2 onScreenPos;
    private Vector2 offScreenPos;
    private Coroutine currentRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        onScreenPos = rectTransform.anchoredPosition;

        // Automatically compute off-screen position based on direction
        Vector2 offset = Vector2.zero;
        switch (direction)
        {
            case SlideDirection.Left:
                offset = new Vector2(-Screen.width, 0f);
                break;
            case SlideDirection.Right:
                offset = new Vector2(Screen.width, 0f);
                break;
            case SlideDirection.Up:
                offset = new Vector2(0f, Screen.height);
                break;
            case SlideDirection.Down:
                offset = new Vector2(0f, -Screen.height);
                break;
            case SlideDirection.Custom:
                offset = customOffset;
                break;
        }

        offScreenPos = onScreenPos + offset;
        rectTransform.anchoredPosition = offScreenPos;
    }

    public void SlideIn()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        gameObject.SetActive(true);
        currentRoutine = StartCoroutine(Slide(offScreenPos, onScreenPos));
    }

    public void SlideOut(System.Action onComplete = null)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(Slide(onScreenPos, offScreenPos, onComplete));
    }

    private IEnumerator Slide(Vector2 start, Vector2 end, System.Action onComplete = null)
    {
        float time = 0f;

        while (time < slideDuration)
        {
            float t = slideCurve.Evaluate(time / slideDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = end;
        currentRoutine = null;
        onComplete?.Invoke();
    }
}