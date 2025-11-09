using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Hover_Info : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private static GameObject infoPanel;
    private TMP_Text nameText;
    private TMP_Text infoText;
    [SerializeField] private Vector2 offset = new Vector2(10, -10);

    private Item_Data currentItem;
    private RectTransform panelRect;
    private Canvas parentCanvas;
    private bool isHovering;
    private bool isMobile;

    void Awake()
    {
        #if UNITY_EDITOR // Force hover behavior in the Editor
            isMobile = false;  // Treat Editor as PC
        #else
            isMobile = Application.isMobilePlatform;
        #endif

        GameObject canvasObj = GameObject.Find("Hover_Info_Canvas");
        if (canvasObj != null)
            infoPanel = canvasObj.transform.Find("HoverInfoPanel")?.gameObject;
        else
            Debug.LogError("[Hover_Info]: Hover_Info_Canvas not found in scene!");

        nameText = infoPanel.transform.Find("NameText")?.GetComponent<TMP_Text>();
        infoText = infoPanel.transform.Find("DetailsText")?.GetComponent<TMP_Text>();
        if (nameText == null || infoText == null)
        {
            Debug.LogError("[Hover_Info]: NameText or DetailsText not found under HoverInfoPanel!");
            return;
        }

        panelRect = infoPanel.GetComponent<RectTransform>();
        parentCanvas = infoPanel.GetComponentInParent<Canvas>();
        infoPanel.SetActive(false);
    }

    void Update()
    {
        // Only follow mouse on PC and if info panel is on
        if (!isMobile && isHovering && infoPanel.activeSelf)
            FollowCursor();
    }

    public void SetItem(Item_Data item)
    {
        currentItem = item;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isMobile)
        {
            isHovering = true;
            ShowInfo();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isMobile)
        {
            isHovering = false;
            HideInfo();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMobile)
        {
            if (infoPanel.activeSelf)
                HideInfo();
            else
                ShowInfo();
        }
    }

    private void ShowInfo()
    {
        if (currentItem == null)
        {
            // Debug.Log("[Hover_Info]: current item is null!");
            return;
        }

        nameText.text = currentItem.Name;

        if (currentItem is Ingredient_Data ing)
        {
            infoText.text =
                // $"Rarity: {ing.tier}\n" +
                $"Description:\n{ing.description}";
        }
        else if (currentItem is Dish_Data dish)
        {
            infoText.text =
                // $"{dish.dishType}\n" +
                $"<sprite name=\"Coin\"> {dish.price:F2}";
                // $"{dish.recipe}";
        }
        else // if it's just an item_data (i.e. elf ring)
            infoText.text = "No additional info available.";

        infoPanel.SetActive(true);
        // LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        FollowCursor(); // Position right away when shown
    }

    private void HideInfo()
    {
        infoPanel.SetActive(false);
    }

    private void FollowCursor()
    {
        if (parentCanvas == null || panelRect == null)
            return;

        // Get local point of mouse on slot rectangle
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out Vector2 localPoint
        );

        // Set hover detail rect to local point of mouse + some offset
        panelRect.localPosition = localPoint + offset;
    }
    
    /// <summary>
    /// Clamps a given local position so that a UI panel (RectTransform) stays fully
    /// within the bounds of its parent canvas.
    /// </summary>
    private Vector2 ClampToCanvas(Vector2 position) 
    {
        RectTransform canvasRect = parentCanvas.transform as RectTransform;
        Vector2 size = panelRect.sizeDelta;
        
        float minX = (-canvasRect.rect.width / 2) + (size.x / 2);
        float maxX = (canvasRect.rect.width / 2) - (size.x / 2);
        float minY = (-canvasRect.rect.height / 2) + (size.y / 2);
        float maxY = (canvasRect.rect.height / 2) - (size.y / 2);
        
        float x = Mathf.Clamp(position.x, minX, maxX);
        float y = Mathf.Clamp(position.y, minY, maxY);

        return new Vector2(x, y); 
    }
}