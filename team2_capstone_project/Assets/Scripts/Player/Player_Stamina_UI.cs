using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Stamina_UI : MonoBehaviour
{
    [SerializeField] private Image staminaFill;
    [SerializeField] private float fadeSpeed = 2f;

    private float targetFill = 1f;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (staminaFill == null)
            staminaFill = GetComponentInChildren<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    public void SetStamina(float normalized)
    {
        targetFill = normalized;
        canvasGroup.alpha = (normalized < 1f) ? 1f : 0f;
    }

    private void Update()
    {
        if (staminaFill == null) return;
        staminaFill.fillAmount = Mathf.MoveTowards(staminaFill.fillAmount, targetFill, Time.deltaTime * fadeSpeed);
    }
}
