// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;
// using Vector2 = UnityEngine.Vector2;
// using Vector3 = UnityEngine.Vector3;
// using UnityEngine.InputSystem;
// using UnityEngine.UI;
// using TMPro;
// // using UnityEngine.UIElements;
// using System;
// using UnityEngine.InputSystem.LowLevel;
// using System.Numerics; //for actions
// using Quaternion = UnityEngine.Quaternion;  // Add this line



// [System.Serializable]
// public class CutLine
// {
//     public Transform startMarker;
//     public Transform endMarker;
//     public bool cutCompleted;
// }

// public enum CuttingState
// {
//     Idle,
//     ShowingLine,
//     KnifeSnapped,
//     WaitingForSwipe,
//     SwipeComplete,
//     ProcessingCut
// }

// public class Chop_Controller : MonoBehaviour
// {
//   [Header("References")]
//   public Camera mainCamera;

//   [Header("Behaviour")]
//   private Vector2 startDragPos;
//   private bool isDragging;

//   public Drag_All drag_all_script;
//   public Knife_Script k_script;

//   [Header("Cut Settings")]
//   public Ingredient_Data ingredient_data_var;
//   public GameObject ingredient_object;
//   private bool hasIngredientData = true;

//   private GameObject currentCuttingLines;
//   public GameObject uiLineRendererPrefab; // Prefab with UILineRenderer script attached
//   public Transform cuttingLinesParent;

//   public Ingredient_Data defaultIngredient; // Assign in Inspector

//   // private Vector3 knife_pos;
//   public bool knife_is_overlapping = false;
//   public RectTransform redZoneForKnife;
//   // private bool wasDragging = false;

//   public List<Vector2> currentLinePoints;
//   [SerializeField] private TextMeshProUGUI temporary_Tutorial;
//   private bool firstSnap = true; // DELETE THIS LATER
//   private bool firstDrop = true; // DELETE THIS LATER

//   [Header("Cutting State")]
//   public CuttingState currentState = CuttingState.Idle;
//   public int currentLineIndex = 0;
//   private List<List<Vector2>> allCuttingLines = new List<List<Vector2>>();


//   [Header("Swipe Detection")]
//   private Vector2 swipeStartPos;
//   private Vector2 lastSwipePos;
//   private bool isDetectingSwipe = false;
//   public float swipeThreshold = 50f;
//   public float swipeAngleTolerance = 30f;

//   // New variables for back-and-forth detection
//   private int swipeDirectionChanges = 0;
//   private Vector2 lastSwipeDirection;
//   private float totalSwipeDistance = 0f;
//   public float requiredSwipeDistance = 300f; // Total distance needed to complete cut
//   public float splitSpeed = 0.5f; // How fast pieces move apart per swipe distance unit


  
//   private Vector3 piece1StartPos;
//   private Vector3 piece2StartPos;
//   private Vector3 piece1TargetOffset;
//   private Vector3 piece2TargetOffset;

//   // References to your ingredient pieces (assign in Inspector or find them)
//   public RectTransform ingredientPiece1;
//   public RectTransform ingredientPiece2;
//   public RectTransform ingredientPiece3;
//   public RectTransform ingredientPiece4;
//   public UILineRenderer lineRenderer;

//   private Vector3 piece1OriginalPos;
//   private Vector3 piece2OriginalPos;
//   private Vector3 piece3OriginalPos;
//   private Vector3 piece4OriginalPos;

//   public Ingredient_Data SetIngredientData(Ingredient_Data ingredientData, GameObject ing_gameOb)
//   {
//     //this function is used to get the ingredientData from DragAll
//     ingredient_data_var = ingredientData;
//     ingredient_object = ing_gameOb;
//     hasIngredientData = true;

//     // Debug.Log("[Chp_ctrller] Received ingredient data: " + ingredient_data_var); //works
//     if (firstDrop)
//     {
//       SetTutorialText("Drag and drop knife over line."); // DELETE LATER
//       firstDrop = false;
//     }
//     return ingredient_data_var;
//   }

//   /// <summary>
//   ///  function to make the lines appear per ingredient
//   /// </summary>
//   private void ShowCuttingLines()
//   {
//     // if (currentCuttingLines != null)
//     // {
//     //     Destroy(currentCuttingLines);
//     // }

//     // cuttingLinesParent = GameObject.Find("Canvas-MinigameElements").transform;
//     // currentCuttingLines = Instantiate(uiLineRendererPrefab, cuttingLinesParent);
//     // lineRenderer = currentCuttingLines.GetComponent<UILineRenderer>();

//     // if (lineRenderer != null)
//     {
//       // Debug.Log("LineRenderer is not null");

//       InitializeCuttingLines();
//       // Show the first line
//       // if (allCuttingLines.Count > 0)
//       {
//         // ShowLine(0);
//         currentState = CuttingState.ShowingLine;
//       }
//     }
//     // else
//     // {
//     //     Debug.LogError("LineRenderer not set");
//     // }

//   }
//   public int cuts_left = 0;
//   //shows the ingredient made from pieces pieced together
//   public void ShowIngredientPiecedTogether()
//   {
//     //show the image of the cut stuff all 
//     //parent canvas is called Canvas-CutGroup
//     Transform parent = GameObject.Find("Canvas-CutGroup").transform;
//     SetIngredientPieces();

//     cuts_left = ingredient_data_var.cutsRequired; //this one changes
//     if (ingredient_data_var.Name == "Uncut Fermented Eye")
//     {
//       //show the image of the cut stuff all 
//       Transform fCutTransform = parent.Find("F_Cut_Group"); // Use Transform.Find instead

//       if (fCutTransform != null)
//       {
//         fCutTransform.gameObject.SetActive(true);
//         Debug.Log("F_Cut_Group should be on cutting board now");
//       }
//       else
//       {
//         Debug.LogError("F_Cut_Group not found as child of Canvas-CutGroup");
//       }
//     }
//     else if (ingredient_data_var.Name == "Uncut Fogshroom")
//     {
//       Transform fCutTransform = parent.Find("Fog_Cut_Group"); // Use Transform.Find instead
//       if (fCutTransform != null)
//       {
//         fCutTransform.gameObject.SetActive(true);
//         Debug.Log("Fog_Cut_Group should be on cutting board now");
//       }
//       else
//       {
//         Debug.LogError("Fog_Cut_Group not found as child of Canvas-CutGroup");
//       }
//     }
//     else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//     {
//       Transform fCutTransform = parent.Find("Fickle_Cut_Group"); // Use Transform.Find instead
//       if (fCutTransform != null)
//       {
//         fCutTransform.gameObject.SetActive(true);
//         fCutTransform.Find("Fkl_Uncut").gameObject.SetActive(false);

//         Debug.Log("Fickle_Cut_Group should be on cutting board now");
//       }
//       else
//       {
//         Debug.LogError("Fickle_Cut_Group not found as child of Canvas-CutGroup");
//       }
//     }
//     else if (ingredient_data_var.Name == "Slime Gelatin")
//     {
//       Transform fCutTransform = parent.Find("Slime_Cut_Group"); // Use Transform.Find instead
//       if (fCutTransform != null)
//       {
//         fCutTransform.gameObject.SetActive(true);
//         fCutTransform.Find("Cut_Slime_R").gameObject.SetActive(true);
//         Debug.Log("Slime_Cut_Group should be on cutting board now");
//       }
//       else
//       {
//         Debug.LogError("Slime_Cut_Group not found as child of Canvas-CutGroup");
//       }
//     }
//   }
//   private bool cuttingLineInitialized = false;
//   private void InitializeCuttingLines()
//   {
//     // allCuttingLines.Clear();
//     Transform parent = GameObject.Find("Canvas-MinigameElements").transform;
//     cuttingLineInitialized = false;

//     if (ingredient_data_var.Name == "Uncut Fermented Eye")
//     {
//       Transform chopLine1 = parent.Find("ChopLine");
//       Transform CLRZ = chopLine1.Find("CL1RedZone");
//       Transform chopLine2 = parent.Find("ChopLine2");
//       Transform CLRZ2 = chopLine2.Find("CL2RedZone");

//       if (cuts_left == 2) //first cutline
//       {//initialize the first cut line for uncut fermented Eye
//         chopLine1.gameObject.SetActive(true);
//         CLRZ.gameObject.SetActive(true);
//         cuttingLineInitialized = true;
//       }
//       else if (cuts_left == 1) //find second cut
//       {
//         //reset 
//         chopLine1.gameObject.SetActive(false);
//         CLRZ.gameObject.SetActive(false);

//         Debug.LogWarning("firstcutDone is done..starting second cut");
//         chopLine2.gameObject.SetActive(true);
//         CLRZ2.gameObject.SetActive(true);
//         cuttingLineInitialized = true;
//       }
//     }
//     else if (ingredient_data_var.Name == "Uncut Fogshroom")
//     {
//       Transform Shroom_CL1 = parent.Find("Shroom_CL1");
//       Transform CLRZSh1 = Shroom_CL1.Find("CLRZSh1");

//       // Get the UILineRenderer component
//       UILineRenderer lineRenderer = Shroom_CL1.GetComponent<UILineRenderer>();


//       if (cuts_left == 3) //first cutline
//       {

//         Shroom_CL1.gameObject.SetActive(true);
//         CLRZSh1.gameObject.SetActive(true);
//         cuttingLineInitialized = true;

//       }
//       // else if (cuts_left == 2)
//       // {
//       //     // Set points for first cut (horizontal example)
//       //     lineRenderer.points = new List<Vector2>
//       //     {
//       //         new Vector2(0, -122.2f),    // Start point
//       //         new Vector2(497.3f, -61.9f)   // End point
//       //     };
//       //     // Update the red zone position too if needed
//       //     RectTransform redZoneRect = CLRZSh1.GetComponent<RectTransform>();
//       //     redZoneRect.anchoredPosition = new Vector2(0, -122.2f); // Adjust as needed
//       //     redZoneRect.rotation = Quaternion.Euler(0, 0, -76f); // Match line angle
//       // }
//       // else if (cuts_left == 1)
//       // {
//       //     lineRenderer.points = new List<Vector2>
//       //     {
//       //         new Vector2(0, -248.5f),    // Start point
//       //         new Vector2(497.3f, -172.8f)   // End point
//       //     };
//       //     // Update red zone for third cut
//       //     RectTransform redZoneRect = CLRZSh1.GetComponent<RectTransform>();
//       //     redZoneRect.anchoredPosition = new Vector2(0, -248.5f);
//       //     redZoneRect.rotation = Quaternion.Euler(0, 0, -76f);
//       // } 
//       // Force the line renderer to update visually
//       lineRenderer.SetVerticesDirty();
//     }
//     else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//     {
//       Transform chopLine1 = parent.Find("Fkl_CL1");
//       Transform CLRZ = chopLine1.Find("CLRZFkl1");
//       Transform chopLine2 = parent.Find("Fkl_CL2");
//       Transform CLRZ2 = chopLine2.Find("CLRZFkl2");

//       if (cuts_left == 2) //first cutline
//       {//initialize the first cut line for uncut fermented Eye
//         chopLine1.gameObject.SetActive(true);
//         CLRZ.gameObject.SetActive(true);
//         cuttingLineInitialized = true;

//       }
//       else if (cuts_left == 1) //find second cut
//       {
//         //reset 
//         chopLine1.gameObject.SetActive(false);
//         CLRZ.gameObject.SetActive(false);

//         Debug.LogWarning("firstcutDone is done..starting second cut");
//         chopLine2.gameObject.SetActive(true);
//         CLRZ2.gameObject.SetActive(true);
//         cuttingLineInitialized = true;
//       }


//     }
//     else if (ingredient_data_var.Name == "Slime Gelatin")
//     {
//       Transform chopLine1 = parent.Find("Slime_CL");
//       Transform CLRZ = chopLine1.Find("CLRZSLIME");
//       chopLine1.gameObject.SetActive(true);
//       CLRZ.gameObject.SetActive(true);
//       cuttingLineInitialized = true;
//     }

//   }

//   private void ShowLine(int lineIndex)
//   {
//     ShowCuttingLines();
//   }

//   private void ClearCuttingLines()
//   {
//     Transform parent = GameObject.Find("Canvas-MinigameElements").transform;
//     if (ingredient_data_var.Name == "Uncut Fermented Eye")
//     {
//       parent.Find("ChopLine2").gameObject.SetActive(false);

//     }
//     else if (ingredient_data_var.Name == "Uncut Fogshroom")
//     {
//       parent.Find("Shroom_CL1").gameObject.SetActive(false);
//     }
//     else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//     {
//       parent.Find("Fkl_CL2").gameObject.SetActive(false);
//     }
//     else if (ingredient_data_var.Name == "Slime Gelatin")
//     {
//       parent.Find("Slime_CL").gameObject.SetActive(false);

//     }
//     currentState = CuttingState.Idle;
//   }

//   public bool firstCutDone;
//   public bool secondCutDone;
//   void Start()
//   {
//     firstCutDone = false;
//     secondCutDone = false;
//     drag_all_script = FindObjectOfType<Drag_All>();
//     // Debug.LogError("Drag_All found in CHop_Controller!");
//     // Set default if no ingredient is set
//     //     if (ingredient_data_var == null && defaultIngredient != null)
//     //     {
//     //         ingredient_data_var = defaultIngredient;
//     // }
//     if (drag_all_script == null)
//     {
//       Debug.LogError("Drag_All not found in CHop_Controller!");
//     }

//     k_script = FindObjectOfType<Knife_Script>();
//     if (k_script == null)
//     {
//       Debug.LogError("knife_Script not found in CHop_Controller!");
//     }


//     //invoked functions
//     k_script.OnDragStart += ShowCuttingLines;
//     k_script.OnKnifeSnapped += HandleKnifeSnapped;
//     k_script.OnDragEnd += HandleKnifeDragEnd;
//     // k_script.OnDragEnd += ClearCuttingLines;

//     // Debug.Log("[Chp_Cntrller] ingredient_data_var = " + ingredient_data_var);

//   }
//   public bool isOverlappingCL1R = false;
//   private void EvaluateChop()
//   {
//     bool isOverlappingCLR = false;
//     Transform CL = null;
//     RectTransform CLR = null;
//     if (ingredient_data_var.Name == "Uncut Fermented Eye" && cuttingLineInitialized)
//     {

//       if (cuts_left == 2)
//       {
//         CL = GameObject.Find("ChopLine").transform;
//         CLR = CL.Find("CL1RedZone").GetComponent<RectTransform>();
//         // isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRect, CLR);
//         isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRectTransform, CLR);
//       }
//       else if (cuts_left == 1)
//       {
//         CL = GameObject.Find("ChopLine2").transform;
//         CLR = CL.Find("CL2RedZone").GetComponent<RectTransform>();
//         // isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRect, CLR);
//         isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRectTransform, CLR);
//         Debug.Log($"CL: {CL}, CLR: {CLR}, isOverlapping: {isOverlappingCLR}");

//       }

//     }
//     else if (ingredient_data_var.Name == "Uncut Fogshroom" && cuttingLineInitialized)
//     {
//       CL = GameObject.Find("Shroom_CL1").transform;
//       CLR = CL.Find("CLRZSh1").GetComponent<RectTransform>();
//       // isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRect, CLR);
//       isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRectTransform, CLR);
//     }
//     else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//     {
//       if (cuts_left == 2)
//       {
//         CL = GameObject.Find("Fkl_CL1").transform;
//         CLR = CL.Find("CLRZFkl1").GetComponent<RectTransform>();
//         // isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRect, CLR);
//         isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRectTransform, CLR);

//       }
//       else if (cuts_left == 1)
//       {
//         CL = GameObject.Find("Fkl_CL2").transform;
//         CLR = CL.Find("CLRZFkl2").GetComponent<RectTransform>();
//         // isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRect, CLR);
//         isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRectTransform, CLR);
//         Debug.Log($"CL: {CL}, CLR: {CLR}, isOverlapping: {isOverlappingCLR}");
//       }
//     }
//     else if (ingredient_data_var.Name == "Slime Gelatin")
//     {
//       CL = GameObject.Find("Slime_CL").transform;
//       CLR = CL.Find("CLRZSLIME").GetComponent<RectTransform>();
//       // isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRect, CLR);
//       isOverlappingCLR = Drag_All.IsOverlappingRotated(k_script.knifeRectTransform, CLR);
//     }
//     else
//     {
//       return;
//     }

//     // Create a small rect at mouse position
//     Vector3 mpos = k_script.knifeRect.localPosition;
//     //if its overlapping
//     //start timer
//     if (isOverlappingCLR)
//     {
//       Debug.Log($"Knife is Overlapping C1LR at Position: {mpos}");
//       k_script.SnapToLine();

//       currentTime += Time.deltaTime;

//     }
//     else
//     { //not overlapping
//       if (currentTime >= 0.1f)
//       {//over this threshold to count as a full cut
//        //cut has been made
//        // Debug.Log("Cut has been made... starting timer");
//         firstCutDone = true;
//         // k_script.knifeImage.raycastTarget = true;
//       }
//       else
//       {
//         currentTime = 0; //reset timer
//       }
//     }
//   }
//   private float currentTime = 0f;
//   void Update()
//   {
//     if (!hasIngredientData) return;
//     //if there isnt something on the cutting board
//     //     GameObject red_zone_found = GameObject.Find("RedZoneForKnife");
//     // if (red_zone_found != null)
//     //     redZoneForKnife = red_zone_found.GetComponent<RectTransform>();
//     // else
//     // {
//     //     Debug.Log("[Chp_contrller] Could not find redZone4Knife!");
//     // }

//     // State machine for cutting process
//     switch (currentState)
//     {
//       case CuttingState.ShowingLine:
//         // Waiting for player to pick up knife
//         EvaluateChop();
//         break;

//       case CuttingState.KnifeSnapped:
//         // Knife is snapped to line, waiting for player to release
//         break;

//       case CuttingState.WaitingForSwipe:
//         // Debug.Log("In Wating for Swip state");
//         if (ingredient_data_var.Name == "Uncut Fermented Eye")
//         {
//           if (cuts_left == 2) // First cut: split all 4 pieces into 2 groups
//           {
//             // Pieces 1 & 2 go left, Pieces 3 & 4 go right
//             RectTransform[] leftSide = new RectTransform[] { ingredientPiece1, ingredientPiece4 };
//             RectTransform[] rightSide = new RectTransform[] { ingredientPiece2, ingredientPiece3 };
//             DetectSwipeMotion(leftSide, rightSide);
//           }
//           else if (cuts_left == 1) // Second cut: split one half (e.g., pieces 1 & 2)
//           {
//             // Piece 1 goes left, Piece 2 goes right
//             RectTransform[] leftSide = new RectTransform[] { ingredientPiece3, ingredientPiece4 };
//             RectTransform[] rightSide = new RectTransform[] { ingredientPiece1, ingredientPiece2 };
//             DetectSwipeMotion(leftSide, rightSide);
//           }
//         }
//         else if (ingredient_data_var.Name == "Uncut Fogshroom")
//         {
//           if (cuts_left == 3) // First cut: split all 4 pieces into 2 groups
//           {
//             // Pieces 1 & 2 go left, Pieces 3 & 4 go right
//             RectTransform[] leftSide = new RectTransform[] { ingredientPiece1, ingredientPiece2 };
//             RectTransform[] rightSide = new RectTransform[] { ingredientPiece3, ingredientPiece4 };
//             DetectSwipeMotion(leftSide, rightSide);
//           }
//         }
//         else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//         {
//           if (cuts_left == 2) // First cut: split all 4 pieces into 2 groups
//           {
//             // Pieces 1 & 2 go left, Pieces 3 & 4 go right
//             RectTransform[] leftSide = new RectTransform[] { ingredientPiece1, ingredientPiece2 };
//             RectTransform[] rightSide = new RectTransform[] { ingredientPiece3, ingredientPiece4 };
//             DetectSwipeMotion(leftSide, rightSide);
//           }
//           else if (cuts_left == 1) // Second cut: split one half (e.g., pieces 1 & 2)
//           {
//             // Piece 1 goes left, Piece 2 goes right
//             RectTransform[] leftSide = new RectTransform[] { ingredientPiece3, ingredientPiece2 };
//             RectTransform[] rightSide = new RectTransform[] { ingredientPiece4 };
//             DetectSwipeMotion(leftSide, rightSide);
//           }
//         }
//         else if (ingredient_data_var.Name == "Slime Gelatin")
//         {
//           RectTransform[] leftSide = new RectTransform[] { ingredientPiece1 };
//           RectTransform[] rightSide = new RectTransform[] { ingredientPiece2 };
//           DetectSwipeMotion(leftSide, rightSide);
//         }
//         break;

//       case CuttingState.SwipeComplete:
//         // Process the cut
//         ProcessCut();
//         break;
//     }
//   }

//   private void SetIngredientPieces()
//   {
//     Transform parent = GameObject.Find("Canvas-CutGroup").transform;

//     if (ingredient_data_var.Name == "Uncut Fermented Eye")
//     {
//       Transform fCutTransform = parent.Find("F_Cut_Group"); // Use Transform.Find instead
//       ingredientPiece1 = fCutTransform.Find("Fermented_Eye_Cut_1").GetComponent<RectTransform>();
//       ingredientPiece2 = fCutTransform.Find("Fermented_Eye_Cut_2").GetComponent<RectTransform>();
//       ingredientPiece3 = fCutTransform.Find("Fermented_Eye_Cut_3").GetComponent<RectTransform>();
//       ingredientPiece4 = fCutTransform.Find("Fermented_Eye_Cut_4").GetComponent<RectTransform>();


//     }
//     else if (ingredient_data_var.Name == "Uncut Fogshroom")
//     {
//       Transform fCutTransform = parent.Find("Fog_Cut_Group"); // Use Transform.Find instead
//       ingredientPiece1 = fCutTransform.Find("Fogshroom_Cut_1").GetComponent<RectTransform>();
//       ingredientPiece2 = fCutTransform.Find("Fogshroom_Cut_2").GetComponent<RectTransform>();
//       ingredientPiece3 = fCutTransform.Find("Fogshroom_Cut_3").GetComponent<RectTransform>();
//       ingredientPiece4 = fCutTransform.Find("Fogshroom_Cut_4").GetComponent<RectTransform>();
//     }
//     else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//     {
//       Transform fcut = parent.Find("Fickle_Cut_Group"); // Use Transform.Find instead
//       Transform fCutTransform = fcut.Find("Fkl_CG"); // Use Transform.Find instead
//       ingredientPiece1 = fCutTransform.Find("Fkl_Cut_1").GetComponent<RectTransform>();
//       ingredientPiece2 = fCutTransform.Find("Fkl_Cut_2").GetComponent<RectTransform>();
//       ingredientPiece3 = fCutTransform.Find("Fkl_Cut_3").GetComponent<RectTransform>();
//       ingredientPiece4 = fCutTransform.Find("Fkl_Cut_4").GetComponent<RectTransform>();
//     }
//     else if (ingredient_data_var.Name == "Slime Gelatin")
//     {
//       Transform fCutTransform = parent.Find("Slime_Cut_Group"); // Use Transform.Find instead

//       ingredientPiece1 = fCutTransform.Find("Cut_Slime_R").GetComponent<RectTransform>();
//       ingredientPiece2 = fCutTransform.Find("Cut_Slime_L").GetComponent<RectTransform>();
//     }

//     //set original positions
//     if (ingredientPiece1 != null) piece1OriginalPos = ingredientPiece1.localPosition;
//     if (ingredientPiece2 != null) piece2OriginalPos = ingredientPiece2.localPosition;
//     if (ingredientPiece3 != null) piece3OriginalPos = ingredientPiece3.localPosition;
//     if (ingredientPiece4 != null) piece4OriginalPos = ingredientPiece4.localPosition;
//   }

//   // private void HandleKnifeDragStart()
//   // {
//   //     if (currentState == CuttingState.Idle && hasIngredientData)
//   //     {
//   //         ShowCuttingLines();
//   //     }
//   // }
//   private void HandleKnifeSnapped()
//   {
//     if (currentState == CuttingState.ShowingLine)
//     {
//       currentState = CuttingState.KnifeSnapped;
//       Debug.Log("Knife snapped to line!");
//       if (firstSnap)
//       {
//         SetTutorialText("Click and drag along the line back and forth until the line disappears.");
//         firstSnap = false;
//       }
//     }
//   }

//   private void HandleKnifeDragEnd()
//   {
//     if (currentState == CuttingState.KnifeSnapped)
//     {
//       // Knife released while snapped - start swipe detection
//       currentState = CuttingState.WaitingForSwipe;
//       isDetectingSwipe = false;
//       Debug.Log("Ready for swipe motion!");
//     }
//   }

//   // Add these class variables:
//   private Vector3[] leftPiecesStartPos;
//   private Vector3[] rightPiecesStartPos;
//   private Vector3 leftSideOffset;
//   private Vector3 rightSideOffset;

//   private void DetectSwipeMotion(RectTransform[] leftSidePieces, RectTransform[] rightSidePieces)
//   {
//     // Debug.Log("In detectSwipeMotion state!");
//     // Start detecting swipe when mouse button is pressed
//     if (Input.GetMouseButtonDown(0) && !isDetectingSwipe)
//     {
//       swipeStartPos = Input.mousePosition;
//       lastSwipePos = swipeStartPos;
//       isDetectingSwipe = true;
//       swipeDirectionChanges = 0;
//       totalSwipeDistance = 0f;
//       lastSwipeDirection = Vector2.zero;

//       // Store initial positions for ALL pieces
//       if (leftSidePieces != null && rightSidePieces != null)
//       {
//         // Store starting positions 
//         leftPiecesStartPos = new Vector3[leftSidePieces.Length];
//         rightPiecesStartPos = new Vector3[rightSidePieces.Length];

//         for (int i = 0; i < leftSidePieces.Length; i++)
//         {
//           if (leftSidePieces[i] != null)
//             leftPiecesStartPos[i] = leftSidePieces[i].localPosition;
//         }

//         for (int i = 0; i < rightSidePieces.Length; i++)
//         {
//           if (rightSidePieces[i] != null)
//             rightPiecesStartPos[i] = rightSidePieces[i].localPosition;
//         }

//         // Calculate split directions
//         float lineRotation = GetLineRotation();
//         Vector2 perpendicular = Quaternion.Euler(0, 0, lineRotation + 90f) * Vector2.up;

//         leftSideOffset = perpendicular * 15f;
//         rightSideOffset = -perpendicular * 15f;
//       }
//     }

//     if (k_script.knife_is_being_dragged && isDetectingSwipe)
//     {
//       Vector2 currentPos = Input.mousePosition;
//       Vector2 swipeDelta = currentPos - lastSwipePos;

//       if (swipeDelta.magnitude > 2f)
//       {
//         Vector2 currentDirection = swipeDelta.normalized;

//         if (lastSwipeDirection != Vector2.zero)
//         {
//           float directionDot = Vector2.Dot(currentDirection, lastSwipeDirection);

//           if (directionDot < -0.5f)
//           {
//             swipeDirectionChanges++;
//             Debug.Log($"Direction change detected! Total changes: {swipeDirectionChanges}");
//           }
//         }

//         totalSwipeDistance += swipeDelta.magnitude;
//         float splitProgress = Mathf.Clamp01(totalSwipeDistance / requiredSwipeDistance);

//         // Move all left side pieces together
//         for (int i = 0; i < leftSidePieces.Length; i++)
//         {
//           if (leftSidePieces[i] != null)
//           {
//             leftSidePieces[i].localPosition = Vector3.Lerp(
//                 leftPiecesStartPos[i],
//                 leftPiecesStartPos[i] + leftSideOffset,
//                 splitProgress
//             );
//           }
//         }

//         // Move all right side pieces together
//         for (int i = 0; i < rightSidePieces.Length; i++)
//         {
//           if (rightSidePieces[i] != null)
//           {
//             rightSidePieces[i].localPosition = Vector3.Lerp(
//                 rightPiecesStartPos[i],
//                 rightPiecesStartPos[i] + rightSideOffset,
//                 splitProgress
//             );
//           }
//         }

//         lastSwipeDirection = currentDirection;

//         if (swipeDirectionChanges >= 2 && totalSwipeDistance >= requiredSwipeDistance)
//         {
//           Debug.Log("Valid cut detected! Swipes: " + swipeDirectionChanges +
//                   ", Distance: " + totalSwipeDistance);
//           isDetectingSwipe = false;
//           cuts_left -= 1;
//           Debug.Log($"Cuts left: {cuts_left}");
//           currentState = CuttingState.SwipeComplete;
//         }

//         lastSwipePos = currentPos;
//       }
//     }

//     if (Input.GetMouseButtonUp(0) && isDetectingSwipe)
//     {
//       isDetectingSwipe = false;

//       if (swipeDirectionChanges < 2 || totalSwipeDistance < requiredSwipeDistance)
//       {
//         Debug.Log("Cut incomplete, resetting pieces");

//         // Reset all pieces
//         if (!firstCutDone)
//         {
//           for (int i = 0; i < leftSidePieces.Length; i++)
//           {
//             if (leftSidePieces[i] != null)
//               leftSidePieces[i].localPosition = leftPiecesStartPos[i];
//           }
//         }

//         if (!secondCutDone)
//         {
//           for (int i = 0; i < rightSidePieces.Length; i++)
//           {
//             if (rightSidePieces[i] != null)
//               rightSidePieces[i].localPosition = rightPiecesStartPos[i];
//           }
//         }
//       }
//     }
//   }
//   private bool IsSwipeDirectionValid(Vector2 swipeDelta)
//   {
//     if (currentLinePoints == null || currentLinePoints.Count < 2) return false;

//     // Calculate line direction
//     Vector2 lineDirection = (currentLinePoints[1] - currentLinePoints[0]).normalized;
//     Vector2 swipeDirection = swipeDelta.normalized;

//     // Calculate angle between swipe and line (check both directions)
//     float angle1 = Vector2.Angle(swipeDirection, lineDirection);
//     float angle2 = Vector2.Angle(swipeDirection, -lineDirection);

//     return angle1 < swipeAngleTolerance || angle2 < swipeAngleTolerance;
//   }

//   private void ProcessCut()
//   {
//     currentState = CuttingState.ProcessingCut;

//     Debug.Log($"Processing cut for line {cuts_left}");

//     // // Change sprite to cut version
//     // Image imageComponent = ingredient_object.GetComponent<Image>();
//     // if (imageComponent != null)
//     // {
//     //     ChangeToCutPiece(imageComponent);
//     // }

//     // Move knife back to original position
//     k_script.ReturnToOriginalPosition();

//     // Check if there are more lines to cut
//     // currentLineIndex++;
//     if (ingredient_data_var.Name == "Uncut Fogshroom")
//     {
//       cuts_left = 0; //TODO Delete this if statement, just shortening the number of cuts required
//     }
//     if (cuts_left > 0)
//     {
//       // Show next line after a short delay
//       StartCoroutine(ShowNextLineAfterDelay(0.75f));
//     }
//     else
//     {
//       // All cuts complete - add ingredient to inventory
//       Debug.Log("All cuts complete");

//       // Clear cutting lines
//       ClearCuttingLines();//TODO
//       HideErrorText();
//       StartCoroutine(CompleteAllCuts());
//     }
//   }

//   private IEnumerator ShowNextLineAfterDelay(float delay)
//   {
//     yield return new WaitForSeconds(delay);
//     currentState = CuttingState.ShowingLine;
//     ShowLine(currentLineIndex);
//   }

//   private IEnumerator CompleteAllCuts()
//   {
//     yield return new WaitForSeconds(1.5f);

//     if (ingredient_data_var != null && ingredient_data_var.makesIngredient.Count > 0)
//     {
//       Debug.Log("Adding ingredient: " + ingredient_data_var.makesIngredient[0].ingredient.Name);
//       Ingredient_Inventory.Instance.AddResources(ingredient_data_var.makesIngredient[0].ingredient.ingredientType, 1);
//     }



//     // Hide the ingredient
//     Transform parent = GameObject.Find("Canvas-CutGroup").transform;
//     HideIngredientPieces();
//     ResetIngredientPiecesToOriginal();

//     // Image imageComponent = parent.GetComponent<Image>();
//     // if (imageComponent != null)
//     // {
//     //     imageComponent.enabled = false;
//     // }



//     // Reset state
//     Drag_All.cuttingBoardActive = false;
//     hasIngredientData = false;
//     currentState = CuttingState.Idle;
//   }

//   private void ResetIngredientPiecesToOriginal()
//   {
//     {
//       if (ingredientPiece1 != null) ingredientPiece1.localPosition = piece1OriginalPos;
//       if (ingredientPiece2 != null) ingredientPiece2.localPosition = piece2OriginalPos;
//       if (ingredientPiece3 != null) ingredientPiece3.localPosition = piece3OriginalPos;
//       if (ingredientPiece4 != null) ingredientPiece4.localPosition = piece4OriginalPos;

//       Debug.Log("Reset all ingredient pieces to original positions");
//     }
//   }
//   private void HideIngredientPieces()
//   {
//     if (ingredient_data_var.Name == "Uncut Fermented Eye")
//     {
//       Transform parent = GameObject.Find("Canvas-CutGroup").transform;
//       Transform cutGroup = GameObject.Find("F_Cut_Group").transform;
//       cutGroup.gameObject.SetActive(false);
//     }
//     else if (ingredient_data_var.Name == "Uncut Fogshroom")
//     {
//       Transform parent = GameObject.Find("Canvas-CutGroup").transform;
//       Transform cutGroup = GameObject.Find("Fog_Cut_Group").transform;
//       cutGroup.gameObject.SetActive(false);
//     }
//     else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//     {
//       Transform parent = GameObject.Find("Canvas-CutGroup").transform;
//       Transform cutGroup = GameObject.Find("Fickle_Cut_Group").transform;
//       cutGroup.gameObject.SetActive(false);
//     }
//     else if (ingredient_data_var.Name == "Slime Gelatin")
//     {
//       Transform parent = GameObject.Find("Canvas-CutGroup").transform;
//       Transform cutGroup = GameObject.Find("Slime_Cut_Group").transform; // Use Transform.Find instead
//       cutGroup.gameObject.SetActive(false);
//     }
//   }
//   public void ChangeToCutPiece(Image imageComponent)
//   {
//     if (ingredient_data_var == null ||
//         ingredient_data_var.CutIngredientImages == null ||
//         ingredient_data_var.CutIngredientImages.Length == 0)
//     {
//       Debug.LogError("No cut images available!");
//       return;
//     }

//     // Use the appropriate cut image based on how many cuts have been made
//     int cutImageIndex = Mathf.Min(currentLineIndex, ingredient_data_var.CutIngredientImages.Length - 1);
//     imageComponent.sprite = ingredient_data_var.CutIngredientImages[cutImageIndex];
//   }


//   public float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
//   {
//     Vector2 ap = p - a;
//     Vector2 ab = b - a;
//     float abSqr = ab.sqrMagnitude;
//     if (abSqr == 0f) return Vector2.Distance(p, a);
//     float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / abSqr);
//     Vector2 closest = a + ab * t;
//     return Vector2.Distance(p, closest);
//   }

//   public float GetLineRotation()
//   {
//     Transform parent = GameObject.Find("Canvas-MinigameElements").transform;
//     Transform RZ = null;
//     if (ingredient_data_var.Name == "Uncut Fermented Eye")
//     {
//       if (cuts_left == 2)
//       {
//         Transform chopline = parent.Find("ChopLine"); // Use Transform.Find 
//         Transform CL1RedZone = chopline.Find("CL1RedZone"); // Use Transform.Find
//         RZ = CL1RedZone;
//       }
//       else if (cuts_left == 1)
//       {
//         Transform chopline = parent.Find("ChopLine2"); // Use Transform.Find 
//         Transform CL2RedZone = chopline.Find("CL2RedZone"); // Use Transform.Find 
//         RZ = CL2RedZone;
//         Debug.Log($"RZ.eulerAngles.z: {RZ.eulerAngles.z}");
//         return 52f; //TODO fix this hardcoded value;
//       }

//     }
//     else if (ingredient_data_var.Name == "Uncut Fogshroom")
//     {
//       Transform chopline = parent.Find("Shroom_CL1"); // Use Transform.Find 
//       Transform CL1RedZone = chopline.Find("CLRZSh1"); // Use Transform.Find
//       RZ = CL1RedZone;
//       // if(cuts_left == 3)
//       {
//         return -76.149f; //TODO fix this hardcoded value
//       }

//     }
//     else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//     {
//       if (cuts_left == 2)
//       {
//         Transform chopline = parent.Find("Fkl_CL1"); // Use Transform.Find 
//         Transform CL1RedZone = chopline.Find("CLRZFkl1"); // Use Transform.Find
//         RZ = CL1RedZone;
//         return 360f;
//       }
//       else if (cuts_left == 1)
//       {
//         Transform chopline = parent.Find("Fkl_CL2"); // Use Transform.Find 
//         Transform CL2RedZone = chopline.Find("CLRZFkl2"); // Use Transform.Find 
//         RZ = CL2RedZone;
//         Debug.Log($"RZ.eulerAngles.z: {RZ.eulerAngles.z}");
//         return 360f; //TODO fix this hardcoded value;
//       }
//     }
//     else if (ingredient_data_var.Name == "Slime Gelatin")
//     {
//       Transform chopline = parent.Find("Slime_CL"); // Use Transform.Find 
//       Transform CL2RedZone = chopline.Find("CLRZSLIME"); // Use Transform.Find 
//       RZ = CL2RedZone;
//       Debug.Log($"RZ.eulerAngles.z: {RZ.eulerAngles.z}");
//       return 360f; //TODO fix this hardcoded value;
//     }


//     float rotation = RZ.eulerAngles.z;
//     return Mathf.Abs(rotation);
//   }

//   public Transform GetRedZone()
//   {
//     Transform parent = GameObject.Find("Canvas-MinigameElements").transform;
//     Transform RZ = null;
//     if (ingredient_data_var.Name == "Uncut Fermented Eye")
//     {
//       if (cuts_left == 2)
//       {
//         Transform chopline = parent.Find("ChopLine"); // Use Transform.Find 
//         Transform CL1RedZone = chopline.Find("CL1RedZone"); // Use Transform.Find
//         RZ = CL1RedZone;
//       }
//       else if (cuts_left == 1)
//       {
//         Transform chopline = parent.Find("ChopLine2"); // Use Transform.Find 
//         Transform CL2RedZone = chopline.Find("CL2RedZone"); // Use Transform.Find 
//         RZ = CL2RedZone;
//       }

//     }
//     else if (ingredient_data_var.Name == "Uncut Fogshroom")
//     {
//       Transform chopline = parent.Find("Shroom_CL1"); // Use Transform.Find 
//       Transform CL1RedZone = chopline.Find("CLRZSh1"); // Use Transform.Find
//       RZ = CL1RedZone;
//     }
//     else if (ingredient_data_var.Name == "Uncut Ficklegourd")
//     {
//       if (cuts_left == 2)
//       {
//         Transform chopline = parent.Find("Fkl_CL1"); // Use Transform.Find 
//         Transform CL1RedZone = chopline.Find("CLRZFkl1"); // Use Transform.Find
//         RZ = CL1RedZone;
//       }
//       else if (cuts_left == 1)
//       {
//         Transform chopline = parent.Find("Fkl_CL2"); // Use Transform.Find 
//         Transform CL2RedZone = chopline.Find("CLRZFkl2"); // Use Transform.Find 
//         RZ = CL2RedZone;
//       }
//     }
//     else if (ingredient_data_var.Name == "Slime Gelatin")
//     {
//       Transform chopline = parent.Find("Slime_CL"); // Use Transform.Find 
//       Transform CL2RedZone = chopline.Find("CLRZSLIME"); // Use Transform.Find 
//       RZ = CL2RedZone;

//     }

//     return RZ;
//   }


//   // DELETE THIS LATER since it is temporary tutorial
//   public void SetTutorialText(string txt)
//   {
//     temporary_Tutorial.text = txt;
//     temporary_Tutorial.gameObject.SetActive(true);
//     // Invoke(nameof(HideErrorText), 2f);
//   }
//   private void HideErrorText()
//   {
//     temporary_Tutorial?.gameObject.SetActive(false);
//   }
// }
