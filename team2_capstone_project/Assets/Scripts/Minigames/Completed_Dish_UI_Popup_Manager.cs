using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Completed_Dish_UI_Popup_Manager : MonoBehaviour
{
    public static Completed_Dish_UI_Popup_Manager instance;
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private float floatUpDistance = 80f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private Vector3 startOffset = new Vector3(0, 80f, 0);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void ShowPopup(string message, Color color)
    {
        if (popupPrefab == null)
        {
            Debug.LogWarning("[Completed_Dish_UI_Popup_Manager] Popup prefab not assigned!");
            return;
        }

        // Instantiate under this manager’s transform (UI space)
        GameObject popup = Instantiate(popupPrefab, transform);
        popup.SetActive(true);

        RectTransform rect = popup.GetComponent<RectTransform>();
        rect.anchoredPosition = startOffset;

        TMP_Text text = popup.GetComponentInChildren<TMP_Text>();
        text.text = message;
        text.color = color;

        CanvasGroup group = popup.GetComponent<CanvasGroup>();
        if (group == null) group = popup.AddComponent<CanvasGroup>();

        StartCoroutine(AnimatePopup(rect, group));
    }

    private IEnumerator AnimatePopup(RectTransform rect, CanvasGroup group)
    {
        float elapsed = 0f;
        Vector2 start = rect.anchoredPosition;
        Vector2 end = start + Vector2.up * floatUpDistance;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            group.alpha = 1f - t;
            yield return null;
        }

        Destroy(rect.gameObject);
    }
}
