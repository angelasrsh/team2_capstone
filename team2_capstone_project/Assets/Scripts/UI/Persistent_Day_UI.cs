using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Persistent_Day_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayLabel;
    [SerializeField] private Image sunImage;
    [SerializeField] private Image moonImage;

    private void Start()
    {
        UpdateDayLabelImmediate();
        RefreshIcons();
    }

    private void RefreshIcons()
    {
        if (Day_Turnover_Manager.Instance == null) return;

        var tod = Day_Turnover_Manager.Instance.currentTimeOfDay;

        sunImage.gameObject.SetActive(tod == Day_Turnover_Manager.TimeOfDay.Morning);
        moonImage.gameObject.SetActive(tod == Day_Turnover_Manager.TimeOfDay.Evening);
    }

    private void OnEnable()
    {
        Day_Turnover_Manager.OnDayEnded += UpdateDayLabel;
        Day_Turnover_Manager.OnDayStarted += RefreshIcons;
    }

    private void OnDisable()
    {
        Day_Turnover_Manager.OnDayEnded -= UpdateDayLabel;
        Day_Turnover_Manager.OnDayStarted -= RefreshIcons;
    }

    private void UpdateDayLabel(Day_Summary_Data data)
    {
        dayLabel.text = $"{data.currentDay}";
        RefreshIcons();
    }

    private void UpdateDayLabelImmediate()
    {
        if (Day_Turnover_Manager.Instance == null) return;
        dayLabel.text = $"{Day_Turnover_Manager.Instance.CurrentDay}";
    }
}
