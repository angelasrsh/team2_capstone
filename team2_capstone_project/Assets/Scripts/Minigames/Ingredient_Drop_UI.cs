using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ingredient_Drop_UI : MonoBehaviour
{
    [Header("Drop Animation Settings")]
    public float dropDistance = 150f;
    public float dropDuration = 0.8f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.2f);
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Size Settings")]
    public float startSize = 0.7f;
    public float endSize = 0.2f;

    private Vector3 startPos;
    private Vector3 endPos;
    private Image image;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Sprite ingredientSprite, Vector3 startOffset)
    {
        if (image == null) image = GetComponent<Image>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        image.sprite = ingredientSprite;
        image.SetNativeSize();

        // Starting offset from parent center (so it drops into cauldron visually)
        rectTransform.anchoredPosition = startOffset;
        startPos = rectTransform.anchoredPosition;
        endPos = startPos + Vector3.down * dropDistance;

        StartCoroutine(DropRoutine());
    }

    private IEnumerator DropRoutine()
    {
        float t = 0f;
        Color originalColor = image.color;

        while (t < dropDuration)
        {
            t += Time.deltaTime;
            float normalized = t / dropDuration;

            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, normalized);

            float scaleFactor = Mathf.Lerp(startSize, endSize, scaleCurve.Evaluate(normalized));
            rectTransform.localScale = Vector3.one * scaleFactor;

            float alpha = alphaCurve.Evaluate(normalized);
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }
}
