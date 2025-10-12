using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
// using UnityEngine.UIElements;
using System;
using UnityEngine.InputSystem.LowLevel; //for actions



[System.Serializable]
public class CutLine
{
    public Transform startMarker;
    public Transform endMarker;
    public bool cutCompleted;
}

public enum CuttingState
{
    Idle,
    ShowingLine,
    KnifeSnapped,
    WaitingForSwipe,
    SwipeComplete,
    ProcessingCut
}

public class Chop_Controller : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;

    [Header("Behaviour")]
    private Vector2 startDragPos;
    private bool isDragging;

    public Drag_All drag_all_script;
    public Knife_Script k_script;

    [Header("Cut Settings")]
    public Ingredient_Data ingredient_data_var;
    public GameObject ingredient_object;
    private bool hasIngredientData = true;

    private GameObject currentCuttingLines;
    public GameObject uiLineRendererPrefab; // Prefab with UILineRenderer script attached
    public Transform cuttingLinesParent;

    public Ingredient_Data defaultIngredient; // Assign in Inspector

    private Vector3 knife_pos;
    public bool knife_is_overlapping = false;
    public RectTransform redZoneForKnife;
    private bool wasDragging = false;

    public List<Vector2> currentLinePoints;


    [Header("Cutting State")]
    public CuttingState currentState = CuttingState.Idle;
    public int currentLineIndex = 0;
    private List<List<Vector2>> allCuttingLines = new List<List<Vector2>>();


    [Header("Swipe Detection")]
    private Vector2 swipeStartPos;
    private bool isDetectingSwipe = false;
    public float swipeThreshold = 50f; // Minimum distance for valid swipe
    public float swipeAngleTolerance = 30f; // Degrees of tolerance for swipe direction

    public UILineRenderer lineRenderer;


    public Ingredient_Data SetIngredientData(Ingredient_Data ingredientData, GameObject ing_gameOb)
    {
        //this function is used to get the ingredientData from DragAll
        ingredient_data_var = ingredientData;
        ingredient_object = ing_gameOb;
        hasIngredientData = true;

        // Debug.Log("[Chp_ctrller] Received ingredient data: " + ingredient_data_var); //works
        return ingredient_data_var;
    }


    /// <summary>
    ///  function to make the lines appear per ingredient
    /// </summary>
    private void ShowCuttingLines()
    {

        if (currentCuttingLines != null)
        {
            Destroy(currentCuttingLines);
        }

        cuttingLinesParent = GameObject.Find("IngredientResize-Canvas").transform;
        currentCuttingLines = Instantiate(uiLineRendererPrefab, cuttingLinesParent);
        lineRenderer = currentCuttingLines.GetComponent<UILineRenderer>();

        if (lineRenderer != null)
        {
            Debug.Log("LineRenderer is not null");

            InitializeCuttingLines();
                 // Show the first line
            if (allCuttingLines.Count > 0)
            {
                ShowLine(0);
                currentState = CuttingState.ShowingLine;
            }
        }
        else
        {
            Debug.LogError("LineRenderer not set");
        }

        //cut Fermented Eye coordinates
        //point 1: x = 0, y = 36.03
        //point 2: x = 268.4, y = 300

        //cut fogshroom
        //line 1: (0.64,166.9), (300, 217.7)
        //line 2: (0.64,166.9),(81.8,121,1), (186.5,104), (280.4,134.3)
        //line 3: (12.1,50.6), (261.5, 75.1)

    }

//shows the ingredient made from pieces pieced together
    public void ShowIngredientPiecedTogether()
    {   
        if (ingredient_data_var.Name == "Uncut Fermented Eye")
        {
            //
            //show the image of the cut stuff all 
            // Transform parent = GameObject.FindAnyObjectByType<
            // GameObject CombinedCutPiecePrefab = Instantiate(ingredient_data_var.CombinedCutPiecePrefab);
            // CombinedCutPiecePrefab.SetActive(true);
            // CombinedCutPiecePrefab.transform.SetParent("Canvas-MinigameElements");
            // Debug.Log("Ingredient should be on cutting board now");
        }
    }
    private void InitializeCuttingLines()
    {
        allCuttingLines.Clear();
        currentLineIndex = 0;

        if (ingredient_data_var.Name == "Uncut Fermented Eye")
        {
            allCuttingLines.Add(new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(386.3f, 382.59f)
            });
        }
        else if (ingredient_data_var.Name == "Uncut Fogshroom")
        {
            // Line 1
            allCuttingLines.Add(new List<Vector2>
            {
                new Vector2(0.64f, 166.9f),
                new Vector2(300f, 217.7f)
            });
            // Line 2 (example - adjust as needed)
            allCuttingLines.Add(new List<Vector2>
            {
                new Vector2(12.1f, 50.6f),
                new Vector2(261.5f, 75.1f)
            });
        }
    }

    private void ShowLine(int lineIndex)
    {
        if (lineIndex < allCuttingLines.Count && lineRenderer != null)
        {
            currentLinePoints = allCuttingLines[lineIndex];
            lineRenderer.points = currentLinePoints;
            currentLineIndex = lineIndex;
        }
    }

    private void ClearCuttingLines()
    {
        if (currentCuttingLines != null)
        {
            Destroy(currentCuttingLines);
            currentCuttingLines = null;
        }
        currentState = CuttingState.Idle;
    }


    void Start()
    {
        drag_all_script = FindObjectOfType<Drag_All>();
        // Debug.LogError("Drag_All found in CHop_Controller!");
        // Set default if no ingredient is set
        //     if (ingredient_data_var == null && defaultIngredient != null)
        //     {
        //         ingredient_data_var = defaultIngredient;
        // }
        if (drag_all_script == null)
        {
            Debug.LogError("Drag_All not found in CHop_Controller!");
        }

        k_script = FindObjectOfType<Knife_Script>();
        if (k_script == null)
        {
            Debug.LogError("knife_Script not found in CHop_Controller!");
        }


        //invoked functions
        k_script.OnDragStart += ShowCuttingLines;
        k_script.OnKnifeSnapped += HandleKnifeSnapped;
        k_script.OnDragEnd += HandleKnifeDragEnd;
        // k_script.OnDragEnd += ClearCuttingLines;

        // Debug.Log("[Chp_Cntrller] ingredient_data_var = " + ingredient_data_var);

    }
    void Update()
    {
        if (!hasIngredientData) return; //if there isnt something on the cutting board
            GameObject red_zone_found = GameObject.Find("RedZoneForKnife");
            if (red_zone_found != null)
                redZoneForKnife = red_zone_found.GetComponent<RectTransform>();
            else
            {
                Debug.Log("[Chp_contrller] Could not find redZone4Knife!");
            }

            // State machine for cutting process
            switch (currentState)
            {
                case CuttingState.ShowingLine:
                    // Waiting for player to pick up knife
                    break;

                case CuttingState.KnifeSnapped:
                    // Knife is snapped to line, waiting for player to release
                    break;

                case CuttingState.WaitingForSwipe:
                    DetectSwipeMotion();
                    break;

                case CuttingState.SwipeComplete:
                    // Process the cut
                    ProcessCut();
                    break;
            }

    }

    private void HandleKnifeDragStart()
    {
        if (currentState == CuttingState.Idle && hasIngredientData)
        {
            ShowCuttingLines();
        }
    }
    private void HandleKnifeSnapped()
    {
        if (currentState == CuttingState.ShowingLine)
        {
            currentState = CuttingState.KnifeSnapped;
            Debug.Log("Knife snapped to line!");
        }
    }

    private void HandleKnifeDragEnd()
    {
        if (currentState == CuttingState.KnifeSnapped)
        {
            // Knife released while snapped - start swipe detection
            currentState = CuttingState.WaitingForSwipe;
            isDetectingSwipe = false;
            Debug.Log("Ready for swipe motion!");
        }
    }


    private void DetectSwipeMotion()
    {
        // Start detecting swipe when mouse button is pressed
        if (Input.GetMouseButtonDown(0) && !isDetectingSwipe)
        {
            swipeStartPos = Input.mousePosition;
            isDetectingSwipe = true;
        }

        // Check swipe while dragging
        if (Input.GetMouseButton(0) && isDetectingSwipe)
        {
            Vector2 currentPos = Input.mousePosition;
            Vector2 swipeDelta = currentPos - swipeStartPos;

            if (swipeDelta.magnitude > swipeThreshold)
            {
                // Check if swipe direction matches cutting line direction
                if (IsSwipeDirectionValid(swipeDelta))
                {
                    Debug.Log("Valid swipe detected!");
                    isDetectingSwipe = false;
                    currentState = CuttingState.SwipeComplete;
                }
            }
        }

        // Reset if mouse released without valid swipe
        if (Input.GetMouseButtonUp(0) && isDetectingSwipe)
        {
            isDetectingSwipe = false;
        }
    }



    private bool IsSwipeDirectionValid(Vector2 swipeDelta)
    {
        if (currentLinePoints == null || currentLinePoints.Count < 2) return false;

        // Calculate line direction
        Vector2 lineDirection = (currentLinePoints[1] - currentLinePoints[0]).normalized;
        Vector2 swipeDirection = swipeDelta.normalized;

        // Calculate angle between swipe and line (check both directions)
        float angle1 = Vector2.Angle(swipeDirection, lineDirection);
        float angle2 = Vector2.Angle(swipeDirection, -lineDirection);

        return angle1 < swipeAngleTolerance || angle2 < swipeAngleTolerance;
    }

    private void ProcessCut()
    {
        currentState = CuttingState.ProcessingCut;
        
        Debug.Log($"Processing cut for line {currentLineIndex}");
        
        // Change sprite to cut version
        Image imageComponent = ingredient_object.GetComponent<Image>();
        if (imageComponent != null)
        {
            ChangeToCutPiece(imageComponent);
        }

        // Move knife back to original position
        k_script.ReturnToOriginalPosition();

        // Check if there are more lines to cut
        currentLineIndex++;
        if (currentLineIndex < allCuttingLines.Count)
        {
            // Show next line after a short delay
            StartCoroutine(ShowNextLineAfterDelay(0.5f));
        }
        else
        {
            // All cuts complete - add ingredient to inventory
            StartCoroutine(CompleteAllCuts());
        }
    }

    private IEnumerator ShowNextLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowLine(currentLineIndex);
        currentState = CuttingState.ShowingLine;
    }

    private IEnumerator CompleteAllCuts()
    {
        yield return new WaitForSeconds(0.75f);

        if (ingredient_data_var != null && ingredient_data_var.makesIngredient.Count > 0)
        {
            Debug.Log("Adding ingredient: " + ingredient_data_var.makesIngredient[0].ingredient.Name);
            Ingredient_Inventory.Instance.AddResources(
                Ingredient_Inventory.Instance.IngrDataToEnum(ingredient_data_var.makesIngredient[0].ingredient), 1);
        }

        // Hide the ingredient
        Image imageComponent = ingredient_object.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.enabled = false;
        }

        // Clear cutting lines
        ClearCuttingLines();
        
        // Reset state
        Drag_All.cuttingBoardActive = false;
        hasIngredientData = false;
        currentState = CuttingState.Idle;
    }

    public void ChangeToCutPiece(Image imageComponent)
    {
        if (ingredient_data_var == null || 
            ingredient_data_var.CutIngredientImages == null || 
            ingredient_data_var.CutIngredientImages.Length == 0)
        {
            Debug.LogError("No cut images available!");
            return;
        }

        // Use the appropriate cut image based on how many cuts have been made
        int cutImageIndex = Mathf.Min(currentLineIndex, ingredient_data_var.CutIngredientImages.Length - 1);
        imageComponent.sprite = ingredient_data_var.CutIngredientImages[cutImageIndex];
    }
    

    public float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ap = p - a;
        Vector2 ab = b - a;
        float abSqr = ab.sqrMagnitude;
        if (abSqr == 0f) return Vector2.Distance(p, a);
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / abSqr);
        Vector2 closest = a + ab * t;
        return Vector2.Distance(p, closest);
    }

    public float GetLineRotation() //TODO: check this because the rotation is not working
    {
        if (currentLinePoints != null && currentLinePoints.Count >= 2)
        {
            Vector2 direction = currentLinePoints[1] - currentLinePoints[0];
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        return 0f;
    }
}
