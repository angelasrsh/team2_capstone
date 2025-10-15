using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class Water_Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector3 originalPos;
    private Canvas rootCanvas;

    [SerializeField] private RectTransform redZone;
    [SerializeField] private GameObject errorText;
    [SerializeField] private Ingredient_Data waterData;
    private Cauldron cauldron;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalParent = transform.parent;
        originalPos = rectTransform.position;

        rootCanvas = GetComponentInParent<Canvas>();

        // find red zone if not assigned
        if (redZone == null)
        {
            var rz = GameObject.Find("RedZone");
            if (rz != null) redZone = rz.GetComponent<RectTransform>();
        }
        if (redZone!= null)
            Debug.Log($"[WaterDrag] Found RedZone: {redZone.name} in scene {SceneManager.GetActiveScene().name}");


        // find cauldron if not assigned
        cauldron = FindObjectOfType<Cauldron>();

        // find error text if not assigned
        if (errorText == null)
        {
            var bg = GameObject.Find("BackgroundCanvas");
            if (bg != null)
            {
                var et = bg.transform.Find("Error_Text");
                if (et != null) errorText = et.gameObject;
            }
        }

        if (waterData == null && Ingredient_Inventory.Instance != null)
            waterData = Ingredient_Inventory.Instance.getWaterData();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // disallow drag if stirring
        cauldron ??= FindObjectOfType<Cauldron>();
        if (cauldron != null && cauldron.IsStirring())
        {
            ShowError("Cannot add water while stirring!");
            return;
        }

        transform.SetParent(transform.root); // render above UI
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("[WaterDrag] OnEndDrag triggered");

        if (redZone == null || rectTransform == null)
        {
            Debug.LogWarning("[WaterDrag] redZone or rectTransform is null! Cannot check overlap.");
            return;
        }

        Debug.Log($"[WaterDrag] Checking overlap between {rectTransform.name} and {redZone?.name}");

        if (Drag_All.IsOverlapping(rectTransform, redZone))
        {
            Debug.Log("[WaterDrag] Overlap detected — adding water to pot!");
            Drag_All.AddWaterToPot();
        }
        else
        {
            Debug.Log("[WaterDrag] No overlap detected!");
        }

        // After check, snap back
        transform.SetParent(originalParent);
        rectTransform.position = originalPos;
    }

    private void ResetPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.position = originalPos;
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            var tmp = errorText.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = message;
            errorText.SetActive(true);
            // auto-hide
            CancelInvoke(nameof(HideError));
            Invoke(nameof(HideError), 3f);
        }
        else
        {
            Debug.LogWarning("[WaterDrag] " + message);
        }
    }

    private void HideError()
    {
        if (errorText != null) errorText.SetActive(false);
    }
}
