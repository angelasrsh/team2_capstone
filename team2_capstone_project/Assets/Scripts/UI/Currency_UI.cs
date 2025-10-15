using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Currency_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateCurrencyDisplay();
    }

    private void OnEnable()
    {
        if (Player_Progress.Instance != null)
            UpdateCurrencyDisplay();
    }

    public void UpdateCurrencyDisplay()
    {
        if (Player_Progress.Instance == null || currencyText == null)
            return;

        currencyText.text = $"{Player_Progress.Instance.GetMoneyAmount():0}";
    }
}
