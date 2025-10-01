using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Persistent_Day_UI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI dayLabel;

    private void OnEnable()
    {
        Day_Turnover_Manager.OnDayEnded += UpdateDayLabel;
    }

    private void OnDisable()
    {
        Day_Turnover_Manager.OnDayEnded -= UpdateDayLabel;
    }

    private void UpdateDayLabel(Day_Summary_Data data)
    {
        dayLabel.text = $"{data.currentDay}";
    }
}
