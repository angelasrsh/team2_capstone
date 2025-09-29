using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Grimoire;

public class Day_Turnover_UI : MonoBehaviour
{
    public GameObject summaryPanel;
    public TextMeshProUGUI persistentDayLabel;

    [Header("Summary-Exclusive UI")]
    public Transform dishesListParent;
    public Transform customersListParent;
    public TextMeshProUGUI totalCurrencyLabel;
    public GameObject listItemPrefab;

    private Screen_Fade blackScreenFade;

    private void Awake()
    {
        summaryPanel.SetActive(false);
    }

    private void OnEnable()
    {
        Day_Turnover_Manager.OnDayEnded += ShowSummary;
    }

    private void OnDisable()
    {
        Day_Turnover_Manager.OnDayEnded -= ShowSummary;
    }

    private void ShowSummary(Day_Summary_Data data)
    {
        StartCoroutine(FadeInRoutine(data));
    }

    private IEnumerator FadeInRoutine(Day_Summary_Data data)
    {
        // Clear old entries
        foreach (Transform child in dishesListParent) Destroy(child.gameObject);
        foreach (Transform child in customersListParent) Destroy(child.gameObject);

        persistentDayLabel.text = $"End of {data.currentDay}";
        totalCurrencyLabel.text = $"Total Earned: {data.totalCurrencyEarned}";

        foreach (var kvp in data.dishesServed)
        {
            var go = Instantiate(listItemPrefab, dishesListParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = $"{kvp.Key.name} x{kvp.Value}";
        }

        foreach (var kvp in data.customersServed)
        {
            var go = Instantiate(listItemPrefab, customersListParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = $"{kvp.Key} x{kvp.Value}";
        }

        blackScreenFade = FindObjectOfType<Screen_Fade>();
        if (blackScreenFade == null)
        {
            Debug.LogWarning("blackScreenFade missing.");
            yield break;
        }

        blackScreenFade.StartCoroutine(blackScreenFade.BlackFadeIn());
        yield return new WaitForSeconds(blackScreenFade.fadeDuration);

        summaryPanel.SetActive(true);

        // use this to fade out stuff eventually
        // float t = 0f;
        // while (t < 1f)
        // {
        //     t += Time.deltaTime;
        //     canvasGroup.alpha = t;
        //     yield return null;
        // }
    }
}