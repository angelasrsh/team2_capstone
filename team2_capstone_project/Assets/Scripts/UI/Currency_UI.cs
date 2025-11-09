using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Currency_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private float countSpeed = 5f;

    private float currentDisplayedAmount;
    private Coroutine countCoroutine;

    private void Start()
    {
        if (Player_Progress.Instance != null)
        {
            currentDisplayedAmount = Player_Progress.Instance.GetMoneyAmount();
            UpdateTextImmediate();
        }
    }

    private void OnEnable()
    {
        if (Player_Progress.Instance != null)
        {
            currentDisplayedAmount = Player_Progress.Instance.GetMoneyAmount();
            UpdateTextImmediate();
        }

        Player_Progress.OnMoneyChanged += AnimateToNewValue;
    }

    private void OnDisable() => Player_Progress.OnMoneyChanged -= AnimateToNewValue;

    private void UpdateTextImmediate()
    {
        if (currencyText != null)
            currencyText.text = $"{currentDisplayedAmount:0}";  // No decimal places
    }

    private void AnimateToNewValue(float newAmount)
    {
        if (countCoroutine != null)
            StopCoroutine(countCoroutine);

        countCoroutine = StartCoroutine(AnimateCount(currentDisplayedAmount, newAmount));
    }

    private IEnumerator AnimateCount(float from, float to)
    {
        float elapsed = 0f;
        while (Mathf.Abs(from - to) > 0.01f)
        {
            elapsed += Time.unscaledDeltaTime * countSpeed;
            currentDisplayedAmount = Mathf.Lerp(from, to, elapsed);
            UpdateTextImmediate();

            if (elapsed >= 1f)
                break;

            yield return null;
        }

        currentDisplayedAmount = to;
        UpdateTextImmediate();
        countCoroutine = null;
    }

    public void ForceRefresh() => AnimateToNewValue(Player_Progress.Instance.GetMoneyAmount());
}
