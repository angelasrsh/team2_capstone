using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Grimoire;

public class Day_Turnover_UI : MonoBehaviour
{
    [Header("Main References")]
    public GameObject summaryPanel;
    public TextMeshProUGUI persistentDayLabel;
    public TextMeshProUGUI totalCurrencyLabel; 
    public Transform listParent;  // Scroll content parent

    [Header("Prefabs")]
    public GameObject entryPrefab;         
    public GameObject headerPrefab;       

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
        Player_Input_Controller.instance?.DisablePlayerInput();
        Pause_Menu.instance?.SetCanPause(false);

        StartCoroutine(FadeInRoutine(data));
    }

    private IEnumerator FadeInRoutine(Day_Summary_Data data)
    {
        if (blackScreenFade == null)
            blackScreenFade = FindObjectOfType<Screen_Fade>();
        if (blackScreenFade != null)
            yield return blackScreenFade.StartCoroutine(blackScreenFade.BlackFadeIn());

        summaryPanel.SetActive(true);

        // Clear existing entries
        foreach (Transform child in listParent)
            Destroy(child.gameObject);

        persistentDayLabel.text = data.nextDay.ToString();
        totalCurrencyLabel.text = $"+{data.totalCurrencyEarned}";

        // Populate in sections
        if (data.dishesServed.Count > 0)
        {
            AddHeader("Dishes Served");
            foreach (var kvp in data.dishesServed)
                AddEntry(kvp.Key.Image, kvp.Key.Name, kvp.Value);
        }

        if (data.customersServed.Count > 0)
        {
            AddHeader("Customers Served");
            foreach (var kvp in data.customersServed)
                AddEntry(null, kvp.Key, kvp.Value);
        }
    }

    private void AddHeader(string text)
    {
        if (headerPrefab == null) return;

        var go = Instantiate(headerPrefab, listParent, false);
        var label = go.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = text;
    }

    private void AddEntry(Sprite icon, string labelText, int quantity)
    {
        if (entryPrefab == null) return;

        var go = Instantiate(entryPrefab, listParent, false);
        var entryUI = go.GetComponent<Day_Summary_Entry_Prefab>();

        if (entryUI != null)
        {
            entryUI.Setup(icon, labelText, quantity);
        }
        else
        {
            Debug.LogWarning("SummaryEntryUI missing on prefab!");
        }
    }

    public void OnContinueButtonPressed()
    {
        StartCoroutine(CloseSummaryRoutine());
    }

    private IEnumerator CloseSummaryRoutine()
    {
        summaryPanel.SetActive(false);

        if (blackScreenFade == null)
            blackScreenFade = FindObjectOfType<Screen_Fade>();

        Audio_Manager.instance.PlaySFX(Audio_Manager.instance.getUpFromBed, 0.35f);
        
        if (blackScreenFade != null)
            yield return blackScreenFade.StartCoroutine(blackScreenFade.BlackFadeOut());

        Player_Input_Controller.instance?.EnablePlayerInput();
        Pause_Menu.instance?.SetCanPause(true);
        // Day_Turnover_Manager.OnDayStarted?.Invoke();
    }
}
