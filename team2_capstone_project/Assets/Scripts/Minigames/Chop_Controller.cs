using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem.LowLevel;
using System.Numerics;
using Quaternion = UnityEngine.Quaternion;
using System.ComponentModel;

// Configuration class for ingredient cutting data
[System.Serializable]
public class IngredientCuttingConfig
{
    public string ingredientName;
    public string cutGroupName;
    public int requiredCuts;
    
    // Cut line configurations per cut index
    public List<CutLineConfig> cutLines = new List<CutLineConfig>();
    
    // Piece names for splitting logic
    public List<string> pieceNames = new List<string>();
    
    // Split configuration per cut
    public List<SplitConfig> splitConfigs = new List<SplitConfig>();
}

[System.Serializable]
public class CutLineConfig
{
    public string lineObjectName;
    public string redZoneObjectName;
    public float lineRotation; // Hardcoded rotation value if needed
}

[System.Serializable]
public class SplitConfig
{
    public int cutIndex; // Which cut this applies to (0 = first cut, 1 = second, etc.)
    public List<int> leftPieceIndices = new List<int>();
    public List<int> rightPieceIndices = new List<int>();
}

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
    public bool hasIngredientData = true; //tells

    private GameObject currentCuttingLines;
    public GameObject uiLineRendererPrefab;
    public Transform cuttingLinesParent;

    public Ingredient_Data defaultIngredient;

    public bool knife_is_overlapping = false;
    public RectTransform redZoneForKnife;

    public List<Vector2> currentLinePoints;
    [SerializeField] private TextMeshProUGUI temporary_Tutorial;
    private bool firstSnap = true;
    private bool firstDrop = true;

    [Header("Cutting State")]
    public CuttingState currentState = CuttingState.Idle;
    public int currentLineIndex = 0;
    private List<List<Vector2>> allCuttingLines = new List<List<Vector2>>();

    [Header("Swipe Detection")]
    private Vector2 swipeStartPos;
    private Vector2 lastSwipePos;
    private bool isDetectingSwipe = false;
    public float swipeThreshold = 50f;
    public float swipeAngleTolerance = 30f;

    private int swipeDirectionChanges = 0;
    private Vector2 lastSwipeDirection;
    private float totalSwipeDistance = 0f;
    public float requiredSwipeDistance = 300f;
    public float splitSpeed = 0.5f;

    private Vector3 piece1StartPos;
    private Vector3 piece2StartPos;
    private Vector3 piece1TargetOffset;
    private Vector3 piece2TargetOffset;

    public RectTransform ingredientPiece1;
    public RectTransform ingredientPiece2;
    public RectTransform ingredientPiece3;
    public RectTransform ingredientPiece4;
    public UILineRenderer lineRenderer;

    private Vector3 piece1OriginalPos;
    private Vector3 piece2OriginalPos;
    private Vector3 piece3OriginalPos;
    private Vector3 piece4OriginalPos;

    public int cuts_left = 0;
    public bool firstCutDone;
    public bool secondCutDone;

    private float currentTime = 0f;

    private Vector3[] leftPiecesStartPos;
    private Vector3[] rightPiecesStartPos;
    private Vector3 leftSideOffset;
    private Vector3 rightSideOffset;

    private bool cuttingLineInitialized = false;
    public bool isOverlappingCL1R = false;

    [Header("Ingredient Configurations")]
    public List<IngredientCuttingConfig> ingredientConfigs = new List<IngredientCuttingConfig>();
    
    private IngredientCuttingConfig currentConfig;
    private RectTransform[] allPieces;
    private Transform canvasMinigameElements;
    private Transform canvasCutGroup;

    public bool addingIngredient = false;
    void Awake()
    {
        // Cache frequently accessed transforms
        canvasMinigameElements = GameObject.Find("Canvas-MinigameElements")?.transform;
        canvasCutGroup = GameObject.Find("Canvas-CutGroup")?.transform;
        
        InitializeIngredientConfigs();
    }

    void Start()
    {
        firstCutDone = false;
        secondCutDone = false;
        drag_all_script = FindObjectOfType<Drag_All>();
        
        if (drag_all_script == null)
        {
            Debug.LogError("Drag_All not found in CHop_Controller!");
        }

        k_script = FindObjectOfType<Knife_Script>();
        if (k_script == null)
        {
            Debug.LogError("knife_Script not found in CHop_Controller!");
        }

        k_script.OnDragStart += ShowCuttingLines;
        k_script.OnKnifeSnapped += HandleKnifeSnapped;
        k_script.OnDragEnd += HandleKnifeDragEnd;
    }

    private void InitializeIngredientConfigs()
    {
        // FERMENTED EYE
        ingredientConfigs.Add(new IngredientCuttingConfig
        {
            ingredientName = "Fermented Eye",
            cutGroupName = "F_Cut_Group",
            requiredCuts = 2,
            cutLines = new List<CutLineConfig>
            {
                new CutLineConfig { lineObjectName = "ChopLine", redZoneObjectName = "CL1RedZone", lineRotation = 0f },
                new CutLineConfig { lineObjectName = "ChopLine2", redZoneObjectName = "CL2RedZone", lineRotation = 52f }
            },
            pieceNames = new List<string> 
            { 
                "Fermented_Eye_Cut_1", 
                "Fermented_Eye_Cut_2", 
                "Fermented_Eye_Cut_3", 
                "Fermented_Eye_Cut_4" 
            },
            splitConfigs = new List<SplitConfig>
            {
                new SplitConfig 
                { 
                    cutIndex = 0, 
                    leftPieceIndices = new List<int> { 0, 3 }, 
                    rightPieceIndices = new List<int> { 1, 2 } 
                },
                new SplitConfig 
                { 
                    cutIndex = 1, 
                    leftPieceIndices = new List<int> { 2, 3 }, 
                    rightPieceIndices = new List<int> { 0, 1 } 
                }
            }
        });

        // FOGSHROOM
        ingredientConfigs.Add(new IngredientCuttingConfig
        {
            ingredientName = "Fogshroom",
            cutGroupName = "Fog_Cut_Group",
            requiredCuts = 3,
            cutLines = new List<CutLineConfig>
            {
                new CutLineConfig { lineObjectName = "Shroom_CL1", redZoneObjectName = "CLRZSh1", lineRotation = -76.149f }
            },
            pieceNames = new List<string> 
            { 
                "Fogshroom_Cut_1", 
                "Fogshroom_Cut_2", 
                "Fogshroom_Cut_3", 
                "Fogshroom_Cut_4" 
            },
            splitConfigs = new List<SplitConfig>
            {
                new SplitConfig 
                { 
                    cutIndex = 0, 
                    leftPieceIndices = new List<int> { 0, 1 }, 
                    rightPieceIndices = new List<int> { 2, 3 } 
                }
            }
        });

        // FICKLEGOURD
        ingredientConfigs.Add(new IngredientCuttingConfig
        {
            ingredientName = "Ficklegourd",
            cutGroupName = "Fickle_Cut_Group",
            requiredCuts = 2,
            cutLines = new List<CutLineConfig>
            {
                new CutLineConfig { lineObjectName = "Fkl_CL1", redZoneObjectName = "CLRZFkl1", lineRotation = 360f },
                new CutLineConfig { lineObjectName = "Fkl_CL2", redZoneObjectName = "CLRZFkl2", lineRotation = 360f }
            },
            pieceNames = new List<string> 
            { 
                "Fkl_CG/Fkl_Cut_1", 
                "Fkl_CG/Fkl_Cut_2", 
                "Fkl_CG/Fkl_Cut_3", 
                "Fkl_CG/Fkl_Cut_4" 
            },
            splitConfigs = new List<SplitConfig>
            {
                new SplitConfig 
                { 
                    cutIndex = 0, 
                    leftPieceIndices = new List<int> { 0, 1 }, 
                    rightPieceIndices = new List<int> { 2, 3 } 
                },
                new SplitConfig 
                { 
                    cutIndex = 1, 
                    leftPieceIndices = new List<int> { 2, 1 }, 
                    rightPieceIndices = new List<int> { 3 } 
                }
            }
        });

        // SLIME GELATIN
        ingredientConfigs.Add(new IngredientCuttingConfig
        {
            ingredientName = "Slime Gelatin",
            cutGroupName = "Slime_Cut_Group",
            requiredCuts = 1,
            cutLines = new List<CutLineConfig>
            {
                new CutLineConfig { lineObjectName = "Slime_CL", redZoneObjectName = "CLRZSLIME", lineRotation = 360f }
            },
            pieceNames = new List<string> 
            { 
                "Cut_Slime_R", 
                "Cut_Slime_L" 
            },
            splitConfigs = new List<SplitConfig>
            {
                new SplitConfig 
                { 
                    cutIndex = 0, 
                    leftPieceIndices = new List<int> { 0 }, 
                    rightPieceIndices = new List<int> { 1 } 
                }
            }
        });
    }

    public Ingredient_Data SetIngredientData(Ingredient_Data ingredientData, GameObject ing_gameOb)
    {
        ingredient_data_var = ingredientData;
        ingredient_object = ing_gameOb;
        hasIngredientData = true;

        currentConfig = ingredientConfigs.Find(c => c.ingredientName == ingredientData.Name);
        
        if (currentConfig == null)
        {
            Debug.LogError($"No configuration found for ingredient: {ingredientData.Name}");
            return ingredient_data_var;
        }

        if (firstDrop)
        {
            SetTutorialText("Drag and drop knife over line.");
            firstDrop = false;
        }
        return ingredient_data_var;
    }

    private void ShowCuttingLines()
    {
        InitializeCuttingLines();
        currentState = CuttingState.ShowingLine;
    }

    public void ShowIngredientPiecedTogether()
    {
        if (currentConfig == null || canvasCutGroup == null) return;

        SetIngredientPieces();
        cuts_left = ingredient_data_var.cutsRequired;

        Transform cutGroupTransform = canvasCutGroup.Find(currentConfig.cutGroupName);
        if (cutGroupTransform != null)
        {
            cutGroupTransform.gameObject.SetActive(true);
            
            // Special case handling for specific ingredients
            if (currentConfig.ingredientName == "Ficklegourd")
            {
                cutGroupTransform.Find("Fkl_Uncut")?.gameObject.SetActive(false);
            }
            else if (currentConfig.ingredientName == "Slime Gelatin")
            {
                cutGroupTransform.Find("Cut_Slime_R")?.gameObject.SetActive(true);
            }
            
            Debug.Log($"{currentConfig.cutGroupName} should be on cutting board now");
        }
        else
        {
            Debug.LogError($"{currentConfig.cutGroupName} not found as child of Canvas-CutGroup");
        }
    }

    private void InitializeCuttingLines()
    {
        if (currentConfig == null || canvasMinigameElements == null) return;

        cuttingLineInitialized = false;
        int cutIndex = currentConfig.requiredCuts - cuts_left;

        if (cutIndex < 0 || cutIndex >= currentConfig.cutLines.Count)
        {
            // Debug.LogError($"Invalid cut index: {cutIndex} for {currentConfig.ingredientName}");
            return;
        }

        CutLineConfig lineConfig = currentConfig.cutLines[cutIndex];
        Transform chopLine = canvasMinigameElements.Find(lineConfig.lineObjectName);
        
        if (chopLine == null)
        {
            Debug.LogError($"Could not find cut line: {lineConfig.lineObjectName}");
            return;
        }

        Transform redZone = chopLine.Find(lineConfig.redZoneObjectName);
        
        if (redZone != null)
        {
            // Deactivate previous cut lines
            DeactivatePreviousCutLines(cutIndex);
            
            chopLine.gameObject.SetActive(true);
            redZone.gameObject.SetActive(true);
            cuttingLineInitialized = true;

            // Update line renderer if present
            UILineRenderer lineRenderer = chopLine.GetComponent<UILineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.SetVerticesDirty();
            }
        }
    }

    private void DeactivatePreviousCutLines(int currentCutIndex)
    {
        if (currentConfig == null || currentCutIndex == 0) return;

        for (int i = 0; i < currentCutIndex; i++)
        {
            if (i < currentConfig.cutLines.Count)
            {
                Transform previousLine = canvasMinigameElements.Find(currentConfig.cutLines[i].lineObjectName);
                Transform previousRedZone = previousLine?.Find(currentConfig.cutLines[i].redZoneObjectName);
                
                if (previousLine != null) previousLine.gameObject.SetActive(false);
                if (previousRedZone != null) previousRedZone.gameObject.SetActive(false);
            }
        }
    }

    private void ShowLine(int lineIndex)
    {
        ShowCuttingLines();
    }

    private void ClearCuttingLines()
    {
        if (currentConfig == null || canvasMinigameElements == null) return;

        foreach (var lineConfig in currentConfig.cutLines)
        {
            Transform line = canvasMinigameElements.Find(lineConfig.lineObjectName);
            if (line != null)
            {
                line.gameObject.SetActive(false);
            }
        }
        
        currentState = CuttingState.Idle;
    }

    void Update()
    {
        if (!hasIngredientData) return;

        switch (currentState)
        {
            case CuttingState.ShowingLine:
                EvaluateChop();
                break;

            case CuttingState.KnifeSnapped:
                break;

            case CuttingState.WaitingForSwipe:
                PerformSwipeDetection();
                break;

            case CuttingState.SwipeComplete:
                ProcessCut();
                break;
        }
    }

    private void PerformSwipeDetection()
    {
        if (currentConfig == null) return;

        int cutIndex = currentConfig.requiredCuts - cuts_left;
        SplitConfig splitConfig = currentConfig.splitConfigs.Find(s => s.cutIndex == cutIndex);
        
        if (splitConfig == null)
        {
            Debug.LogError($"No split config found for cut index {cutIndex}");
            return;
        }

        RectTransform[] leftSide = GetPiecesByIndices(splitConfig.leftPieceIndices);
        RectTransform[] rightSide = GetPiecesByIndices(splitConfig.rightPieceIndices);
        
        DetectSwipeMotion(leftSide, rightSide);
    }

    private RectTransform[] GetPiecesByIndices(List<int> indices)
    {
        if (allPieces == null) return new RectTransform[0];

        List<RectTransform> pieces = new List<RectTransform>();
        foreach (int index in indices)
        {
            if (index >= 0 && index < allPieces.Length && allPieces[index] != null)
            {
                pieces.Add(allPieces[index]);
            }
        }
        return pieces.ToArray();
    }

    private void SetIngredientPieces()
    {
        if (currentConfig == null || canvasCutGroup == null) return;

        Transform cutGroupTransform = canvasCutGroup.Find(currentConfig.cutGroupName);
        if (cutGroupTransform == null)
        {
            Debug.LogError($"Could not find cut group: {currentConfig.cutGroupName}");
            return;
        }

        List<RectTransform> pieces = new List<RectTransform>();
        
        foreach (string pieceName in currentConfig.pieceNames)
        {
            Transform pieceTransform = cutGroupTransform.Find(pieceName);
            if (pieceTransform != null)
            {
                pieces.Add(pieceTransform.GetComponent<RectTransform>());
            }
            else
            {
                Debug.LogWarning($"Could not find piece: {pieceName}");
            }
        }

        allPieces = pieces.ToArray();

        // Assign to legacy piece variables for backwards compatibility
        if (allPieces.Length > 0) ingredientPiece1 = allPieces[0];
        if (allPieces.Length > 1) ingredientPiece2 = allPieces[1];
        if (allPieces.Length > 2) ingredientPiece3 = allPieces[2];
        if (allPieces.Length > 3) ingredientPiece4 = allPieces[3];

        // Store original positions
        if (ingredientPiece1 != null) piece1OriginalPos = ingredientPiece1.localPosition;
        if (ingredientPiece2 != null) piece2OriginalPos = ingredientPiece2.localPosition;
        if (ingredientPiece3 != null) piece3OriginalPos = ingredientPiece3.localPosition;
        if (ingredientPiece4 != null) piece4OriginalPos = ingredientPiece4.localPosition;
    }

    private void EvaluateChop()
    {
        if (currentConfig == null || !cuttingLineInitialized) return;

        int cutIndex = currentConfig.requiredCuts - cuts_left;
        if (cutIndex < 0 || cutIndex >= currentConfig.cutLines.Count) return;

        CutLineConfig lineConfig = currentConfig.cutLines[cutIndex];
        Transform chopLine = canvasMinigameElements?.Find(lineConfig.lineObjectName);
        
        if (chopLine == null) return;

        Transform redZone = chopLine.Find(lineConfig.redZoneObjectName);
        RectTransform redZoneRect = redZone?.GetComponent<RectTransform>();
        
        if (redZoneRect == null) return;

        bool isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRectTransform, redZoneRect);

        if (isOverlappingCLR)
        {
            Debug.Log($"Knife is Overlapping {lineConfig.redZoneObjectName}");
            k_script.SnapToLine();
            currentTime += Time.deltaTime;
        }
        else
        {
            if (currentTime >= 0.1f)
            {
                firstCutDone = true;
            }
            else
            {
                currentTime = 0;
            }
        }
    }

    private void HandleKnifeSnapped()
    {
        if (currentState == CuttingState.ShowingLine)
        {
            currentState = CuttingState.KnifeSnapped;
            Debug.Log("Knife snapped to line!");
            if (firstSnap)
            {
                SetTutorialText("Click and drag along the line back and forth until the line disappears.");
                firstSnap = false;
            }
        }
    }

    private void HandleKnifeDragEnd()
    {
        if (currentState == CuttingState.KnifeSnapped)
        {
            currentState = CuttingState.WaitingForSwipe;
            isDetectingSwipe = false;
            Debug.Log("Ready for swipe motion!");
        }
    }

    private void DetectSwipeMotion(RectTransform[] leftSidePieces, RectTransform[] rightSidePieces)
    {
        if (Input.GetMouseButtonDown(0) && !isDetectingSwipe)
        {
            swipeStartPos = Input.mousePosition;
            lastSwipePos = swipeStartPos;
            isDetectingSwipe = true;
            swipeDirectionChanges = 0;
            totalSwipeDistance = 0f;
            lastSwipeDirection = Vector2.zero;

            if (leftSidePieces != null && rightSidePieces != null)
            {
                leftPiecesStartPos = new Vector3[leftSidePieces.Length];
                rightPiecesStartPos = new Vector3[rightSidePieces.Length];

                for (int i = 0; i < leftSidePieces.Length; i++)
                {
                    if (leftSidePieces[i] != null)
                        leftPiecesStartPos[i] = leftSidePieces[i].localPosition;
                }

                for (int i = 0; i < rightSidePieces.Length; i++)
                {
                    if (rightSidePieces[i] != null)
                        rightPiecesStartPos[i] = rightSidePieces[i].localPosition;
                }

                float lineRotation = GetLineRotation();
                Vector2 perpendicular = Quaternion.Euler(0, 0, lineRotation + 90f) * Vector2.up;

                leftSideOffset = perpendicular * 15f;
                rightSideOffset = -perpendicular * 15f;
            }
        }

        if (k_script.knife_is_being_dragged && isDetectingSwipe)
        {
            Vector2 currentPos = Input.mousePosition;
            Vector2 swipeDelta = currentPos - lastSwipePos;

            if (swipeDelta.magnitude > 2f)
            {
                Vector2 currentDirection = swipeDelta.normalized;

                if (lastSwipeDirection != Vector2.zero)
                {
                    float directionDot = Vector2.Dot(currentDirection, lastSwipeDirection);

                    if (directionDot < -0.5f)
                    {
                        swipeDirectionChanges++;
                        Debug.Log($"Direction change detected! Total changes: {swipeDirectionChanges}");
                    }
                }

                totalSwipeDistance += swipeDelta.magnitude;
                float splitProgress = Mathf.Clamp01(totalSwipeDistance / requiredSwipeDistance);

                for (int i = 0; i < leftSidePieces.Length; i++)
                {
                    if (leftSidePieces[i] != null)
                    {
                        leftSidePieces[i].localPosition = Vector3.Lerp(
                            leftPiecesStartPos[i],
                            leftPiecesStartPos[i] + leftSideOffset,
                            splitProgress
                        );
                    }
                }

                for (int i = 0; i < rightSidePieces.Length; i++)
                {
                    if (rightSidePieces[i] != null)
                    {
                        rightSidePieces[i].localPosition = Vector3.Lerp(
                            rightPiecesStartPos[i],
                            rightPiecesStartPos[i] + rightSideOffset,
                            splitProgress
                        );
                    }
                }

                lastSwipeDirection = currentDirection;

                if (swipeDirectionChanges >= 2 && totalSwipeDistance >= requiredSwipeDistance)
                {
                    Debug.Log("Valid cut detected! Swipes: " + swipeDirectionChanges +
                            ", Distance: " + totalSwipeDistance);
                    isDetectingSwipe = false;
                    cuts_left -= 1;
                    Debug.Log($"Cuts left: {cuts_left}");
                    currentState = CuttingState.SwipeComplete;
                }

                lastSwipePos = currentPos;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDetectingSwipe)
        {
            isDetectingSwipe = false;

            if (swipeDirectionChanges < 2 || totalSwipeDistance < requiredSwipeDistance)
            {
                Debug.Log("Cut incomplete, resetting pieces");

                if (!firstCutDone)
                {
                    for (int i = 0; i < leftSidePieces.Length; i++)
                    {
                        if (leftSidePieces[i] != null)
                            leftSidePieces[i].localPosition = leftPiecesStartPos[i];
                    }
                }

                if (!secondCutDone)
                {
                    for (int i = 0; i < rightSidePieces.Length; i++)
                    {
                        if (rightSidePieces[i] != null)
                            rightSidePieces[i].localPosition = rightPiecesStartPos[i];
                    }
                }
            }
        }
    }

    private bool IsSwipeDirectionValid(Vector2 swipeDelta)
    {
        if (currentLinePoints == null || currentLinePoints.Count < 2) return false;

        Vector2 lineDirection = (currentLinePoints[1] - currentLinePoints[0]).normalized;
        Vector2 swipeDirection = swipeDelta.normalized;

        float angle1 = Vector2.Angle(swipeDirection, lineDirection);
        float angle2 = Vector2.Angle(swipeDirection, -lineDirection);

        return angle1 < swipeAngleTolerance || angle2 < swipeAngleTolerance;
    }

    private void ProcessCut()
    {
        currentState = CuttingState.ProcessingCut;

        Debug.Log($"Processing cut for line {cuts_left}");

        k_script.ReturnToOriginalPosition();

        // Temporary shortcut for Fogshroom
        if (currentConfig?.ingredientName == "Fogshroom")
        {
            cuts_left = 0;
        }

        if (cuts_left > 0)
        {
            StartCoroutine(ShowNextLineAfterDelay(0.75f));
        }
        else
        {
            Debug.Log("All cuts complete");
            ClearCuttingLines();
            // HideErrorText();
            StartCoroutine(CompleteAllCuts());
        }
    }

    private IEnumerator ShowNextLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentState = CuttingState.ShowingLine;
        ShowLine(currentLineIndex);
    }

    private IEnumerator CompleteAllCuts()
    {
        yield return new WaitForSeconds(1.5f);
        if (ingredient_data_var != null && ingredient_data_var.makesIngredient.Count > 0)
        {
            addingIngredient = true;
            Debug.Log("Adding ingredient: " + ingredient_data_var.makesIngredient[0].ingredient.Name);
            Ingredient_Inventory.Instance.AddResources(ingredient_data_var.makesIngredient[0].ingredient.ingredientType, 1);
        }

        HideIngredientPieces();
        ResetIngredientPiecesToOriginal();

        Drag_All.cuttingBoardActive = false;
        Drag_All.canDrag = true;
        hasIngredientData = false;
        currentState = CuttingState.Idle;
        addingIngredient = false;
    }

    private void ResetIngredientPiecesToOriginal()
    {
        if (ingredientPiece1 != null) ingredientPiece1.localPosition = piece1OriginalPos;
        if (ingredientPiece2 != null) ingredientPiece2.localPosition = piece2OriginalPos;
        if (ingredientPiece3 != null) ingredientPiece3.localPosition = piece3OriginalPos;
        if (ingredientPiece4 != null) ingredientPiece4.localPosition = piece4OriginalPos;

        Debug.Log("Reset all ingredient pieces to original positions");
    }

    private void HideIngredientPieces()
    {
        if (currentConfig == null || canvasCutGroup == null) return;

        Transform cutGroup = canvasCutGroup.Find(currentConfig.cutGroupName);
        if (cutGroup != null)
        {
            cutGroup.gameObject.SetActive(false);
        }
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

    public float GetLineRotation()
    {
        if (currentConfig == null || canvasMinigameElements == null) return 0f;

        int cutIndex = currentConfig.requiredCuts - cuts_left;
        if (cutIndex < 0 || cutIndex >= currentConfig.cutLines.Count) return 0f;

        CutLineConfig lineConfig = currentConfig.cutLines[cutIndex];
        Transform redZone = GetRedZone();
        
        if (redZone != null && lineConfig.lineRotation == 0f)
        {
            // Use actual rotation if no hardcoded value
            float rotation = redZone.eulerAngles.z;
            return Mathf.Abs(rotation);
        }
        
        // Return hardcoded rotation value
        return lineConfig.lineRotation;
    }

    public Transform GetRedZone()
    {
        if (currentConfig == null || canvasMinigameElements == null) return null;

        int cutIndex = currentConfig.requiredCuts - cuts_left;
        if (cutIndex < 0 || cutIndex >= currentConfig.cutLines.Count) return null;

        CutLineConfig lineConfig = currentConfig.cutLines[cutIndex];
        Transform chopLine = canvasMinigameElements.Find(lineConfig.lineObjectName);
        
        if (chopLine == null) return null;

        return chopLine.Find(lineConfig.redZoneObjectName);
    }

    // Tutorial text helpers
    public void SetTutorialText(string txt)
    {
        temporary_Tutorial.text = txt;
        temporary_Tutorial.gameObject.SetActive(true);
    }

    private void HideErrorText()
    {
        temporary_Tutorial?.gameObject.SetActive(false);
    }
}

/* ================================================================================
 * HOW TO ADD NEW INGREDIENTS:
 * ================================================================================
 * 
 * 1. In the InitializeIngredientConfigs() method, add a new IngredientCuttingConfig:
 * 
 * ingredientConfigs.Add(new IngredientCuttingConfig
 * {
 *     ingredientName = "Your Ingredient Name",  // Must match Ingredient_Data.Name
 *     cutGroupName = "YourCutGroup_Name",       // Parent GameObject in Canvas-CutGroup
 *     requiredCuts = 2,                          // Number of cuts needed
 *     
 *     cutLines = new List<CutLineConfig>
 *     {
 *         // One config for each cut
 *         new CutLineConfig 
 *         { 
 *             lineObjectName = "YourLine1",      // GameObject in Canvas-MinigameElements
 *             redZoneObjectName = "YourRedZone1", // Child of line object
 *             lineRotation = 45f                  // Hardcoded rotation (use 0f for auto)
 *         },
 *         new CutLineConfig 
 *         { 
 *             lineObjectName = "YourLine2", 
 *             redZoneObjectName = "YourRedZone2", 
 *             lineRotation = 90f 
 *         }
 *     },
 *     
 *     pieceNames = new List<string> 
 *     { 
 *         // Names of piece GameObjects (children of cutGroupName)
 *         "Piece_1", 
 *         "Piece_2", 
 *         "Piece_3", 
 *         "Piece_4" 
 *     },
 *     
 *     splitConfigs = new List<SplitConfig>
 *     {
 *         // One config for each cut
 *         new SplitConfig 
 *         { 
 *             cutIndex = 0,  // First cut (0-indexed)
 *             leftPieceIndices = new List<int> { 0, 1 },  // Pieces that move left
 *             rightPieceIndices = new List<int> { 2, 3 }  // Pieces that move right
 *         },
 *         new SplitConfig 
 *         { 
 *             cutIndex = 1,  // Second cut
 *             leftPieceIndices = new List<int> { 0 }, 
 *             rightPieceIndices = new List<int> { 1 } 
 *         }
 *     }
 * });
 * 
 * 2. Ensure your scene hierarchy matches:
 *    - Canvas-MinigameElements/
 *      - YourLine1/
 *        - YourRedZone1
 *      - YourLine2/
 *        - YourRedZone2
 *    
 *    - Canvas-CutGroup/
 *      - YourCutGroup_Name/
 *        - Piece_1
 *        - Piece_2
 *        - Piece_3
 *        - Piece_4
 * 
 * 3. That's it! No code changes needed elsewhere.
 * 
 * ================================================================================
 * NOTES ON HARDCODED VALUES:
 * ================================================================================
 * 
 * The following values have been extracted to configuration:
 * 
 * - Line Rotations (per ingredient/cut):
 *   - Fermented Eye Cut 1: 0f (auto-detect)
 *   - Fermented Eye Cut 2: 52f
 *   - Fogshroom: -76.149f
 *   - Ficklegourd Cut 1: 360f
 *   - Ficklegourd Cut 2: 360f
 *   - Slime Gelatin: 360f
 * 
 * - Split Directions (which pieces move left/right):
 *   - Defined in splitConfigs per ingredient
 * 
 * - GameObject Names:
 *   - All UI element names are in the configuration
 *   - Can be changed without touching code
 * 
 * - Constants (still in class variables):
 *   - swipeThreshold = 50f
 *   - swipeAngleTolerance = 30f
 *   - requiredSwipeDistance = 300f
 *   - splitSpeed = 0.5f
 *   - Split offset distance = 15f (in DetectSwipeMotion)
 * 
 * ================================================================================
 */