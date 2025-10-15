using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Grimoire;

public class UI_Interaction_Fill : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float fadeSpeed = 1.5f;
    private float targetFill = 0f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponentInChildren<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        Interactable_Object.OnGlobalHoldProgress += HandleProgress;
    }

    private void OnDisable()
    {
        Interactable_Object.OnGlobalHoldProgress -= HandleProgress;
    }

    private void HandleProgress(float progress)
    {
        targetFill = progress;
        canvasGroup.alpha = (progress > 0f) ? 1f : 0f;
    }

    private void Update()
    {
        if (fillImage == null) return;

        fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFill, Time.deltaTime * fadeSpeed);
    }
}
