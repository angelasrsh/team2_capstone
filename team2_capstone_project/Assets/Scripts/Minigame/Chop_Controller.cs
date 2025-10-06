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

public class Chop_Controller : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    // public GameObject ingredient;  
    public CutLine[] cutLines;

    [Header("Settings")]
    public float minChopDistance = 100f; 
    public float minChopSpeed = 800f;   
    public Vector2 requiredDirection = new Vector2(0, -1); 
    public float directionTolerance = 0.7f;
    public float lineTolerancePixels = 50f; 

    [Header("Behaviour")]
    public bool requireSequential = true; // only check the first uncut line

    private Vector2 startDragPos;
    private float startTime;
    private bool isDragging;
    public bool isActive = false;

    public Drag_All drag_all_script;
    public Knife_Script k_script;
    
    [Header("Cut Settings")]
    public GameObject cutPrefab;
    public GameObject cutImagePrefab; // Prefab for the cut versions of the image

    public Ingredient_Data ingredient_data_var;
    public GameObject ingredient_object;
    private bool hasIngredientData = true;

    private GameObject currentCuttingLines;
    public GameObject uiLineRendererPrefab; // Prefab with UILineRenderer script attached
    public Transform cuttingLinesParent;

    public Ingredient_Data defaultIngredient; // Assign in Inspector
    private Vector2 swipeStart;

    private Vector3 knife_pos;
    public bool knife_is_overlapping = false;
    public RectTransform redZoneForKnife;
    public bool in_inventory = false;
    private bool wasDragging = false;

    private GameObject knifePoint;
    public List<Vector2> newPoints;



    public Ingredient_Data SetIngredientData(Ingredient_Data ingredientData, GameObject ing_gameOb)
    {
        //this function is used to get the ingredientData from DragAll
        ingredient_data_var = ingredientData;
        ingredient_object = ing_gameOb;
        hasIngredientData = true;

        // Debug.Log("[Chp_ctrller] Received ingredient data: " + ingredient_data_var); //works
        return ingredient_data_var;
    }


    public UILineRenderer lineRenderer;
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
        // cuttingLinesParent.anchorMin = new Vector2(1, 1);
        // cuttingLinesParent.anchorMax = new Vector2(1, 1);
        // instantiate new cutting lines
        currentCuttingLines = Instantiate(uiLineRendererPrefab, cuttingLinesParent);
        // Immediately configure after instantiation
        lineRenderer = currentCuttingLines.GetComponent<UILineRenderer>();
        
        if (lineRenderer != null)
        {
            Debug.Log("LineRendere is not null");
            if (ingredient_data_var.Name == "Uncut Fermented Eye")
            {
                Debug.Log("Found Uncut Fermented eye");

                //first line
                newPoints = new List<Vector2>
                {
                    new Vector2(0f, 0f),
                    new Vector2(386.3f, 382.59f),
                };
                lineRenderer.points = newPoints;

            }
            else if (ingredient_data_var.Name == "Uncut Fogshroom")
            {
                newPoints = new List<Vector2>
                {
                    new Vector2(0.64f,166.9f),
                    new Vector2(300f, 217.7f),
                };
                lineRenderer.points = newPoints;

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


    private void ClearCuttingLines()
    {
        if (currentCuttingLines != null)
        {
            Destroy(currentCuttingLines);
            currentCuttingLines = null;
        }
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
        k_script.OnDragEnd += ClearCuttingLines;

        // Debug.Log("[Chp_Cntrller] ingredient_data_var = " + ingredient_data_var);

    }
    void Update()
    {
        {
            //Spawn the cut lines when the knife gets dragged
            if (k_script.knife_is_being_dragged == true && !wasDragging) //if the knife is being dragged from Knife Script
            {
                //TODO nice to have = swipeStart  = to position of the little box on the knife that i made instead of mous position

                uiLineRendererPrefab.SetActive(true);
                // swipeStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // startDragPos = Mouse.current.position.ReadValue();
            }
            // else if (k_script.knife_is_being_dragged == false && wasDragging)
            // {
            //     // Vector2 endDragPos = Mouse.current.position.ReadValue();
            //     // SpawnCut();

            //     // EvaluateChop(startDragPos, endDragPos, duration);
            // }
            wasDragging = isDragging; // set the is dragging to was dragging value and the first if statement wont run again


            if (hasIngredientData)
            {
                GameObject red_zone_found = GameObject.Find("RedZoneForKnife");
                if (red_zone_found != null)
                    redZoneForKnife = red_zone_found.GetComponent<RectTransform>();
                else
                {
                    Debug.Log("[Chp_contrller] Could not find redZone4Knife!");
                }
                knife_pos = k_script.currKnifePosition;


                if (!knife_is_overlapping && Drag_All.IsOverlapping(k_script.knifeRectTransform, redZoneForKnife))
                {
                    Debug.Log("[Chp_cntrller] Knife is overlapping redZoneForKnife");
                    knife_is_overlapping = true;
                }
                else
                {
                    knife_is_overlapping = false;

                }

                if (knife_is_overlapping == true && ingredient_object != null)
                {
                    //this if statement is to change to cut sprite when knife is overealpping properly
                    // Debug.Log("went inside the chopscript.knife_is_overlapping if statement");

                    // Get the Image component from the transform, then change the sprite
                    Image imageComponent = ingredient_object.GetComponent<Image>();


                    //if the image of the sprite exists on the cutting board
                    if (imageComponent != null)
                    {
                        //this assumes that the lines are already showing, change the sprite
                        EvaluateChop(imageComponent);
                        
                    }

                    //checks if item is in inventory and if its cut version cant make something
                    // and add it to the inventory
                    if (Drag_All.cuttingBoardActive == true) //something is on the board
                    {
                        //k_script.currentTime > 0.08f && k_script.dist <= 0.9f
                        if (k_script.dist <= 0.9f)
                        {
                            Debug.Log("Ingredient adding name: " + Ingredient_Inventory.Instance.IngrDataToEnum(ingredient_data_var.makesIngredient[0].ingredient));

                            Ingredient_Inventory.Instance.AddResources(Ingredient_Inventory.Instance.IngrDataToEnum(ingredient_data_var.makesIngredient[0].ingredient), 1);

                            StartCoroutine(DelayedActions());

                            IEnumerator DelayedActions()
                            {
                                yield return new WaitForSeconds(0.75f); // Wait .75 seconds

                                imageComponent.enabled = false;
                            }

                            Drag_All.cuttingBoardActive = false;
                        }

                    }

                }
            }
            
        }
        
    }


    public void ChangeToCutPiece(Image imageComponent)
    {
        //null checks
        Debug.Log("[SpawnCut] initiated.");
        // Check if the array exists and has elements
        if (ingredient_data_var == null)
        {
            Debug.LogError("ingredient_data_var is null!");
            return;
        }

        if (ingredient_data_var.CutIngredientImages == null)
        {
            Debug.LogError("CutIngredientImages array is null!");
            return;
        }

        if (ingredient_data_var.CutIngredientImages.Length == 0)
        {
            Debug.LogError("CutIngredientImages array is empty!");
            return;
        }
        //end of null checks

        // Use a cut image if available
        if (ingredient_data_var.CutIngredientImages.Length > 0)
        {
            Debug.Log("length of cut images is greater than 0");
            ChooseCutImage(imageComponent); //depending on the ingredient and the line position you need to change the sprite
        }

    }
    //depending on the ingredient and the line position you need to change the sprite
    private void ChooseCutImage(Image imgComp)
    {

        if (newPoints != null) //if the first line is not null
        {   //use the cut sprite after first line is cut
            imgComp.sprite = ingredient_data_var.CutIngredientImages[0];
        }
        else
        { //use default image
            imgComp.sprite = ingredient_data_var.Image;

        }
        k_script.currentTime = 0f; //reset the timer 
        return;
    //TODO: add if statements if the second or third line is cut


    }

    /// <summary>
    /// Check if the knife pos is close to any of the chop lines
    /// </summary>
    /// 
    /// 1. get knife rect position
    /// 2. compare knife rect position to the line 
    /// 3. if knife is on the line, then start timer
    /// 4. end timer after .02 seconds
    /// 5. spawn the cut version depending on which line was cut

    private void EvaluateChop(Image imageComp)
    {
        // float dist = DistancePointToSegment(kPoint, lineRenderer.points[0], lineRenderer.points[1]); //compare knife rect position to line1;
        if (k_script.dist <= 2f && k_script.seconds > 0.2f) //after some time while dragging
        {
            Debug.Log("Going into the if state in Eval Chop");
            ChangeToCutPiece(imageComp);
            
        }

    }


    // Convert markers to screen space and compute segment-to-segment distance (in pixels)
    private bool IsChopNearLine_ScreenSpace(Vector2 swipeStart, Vector2 swipeEnd, CutLine line)
    {
        Vector2 lineStartScreen = mainCamera.WorldToScreenPoint(line.startMarker.position);
        Vector2 lineEndScreen = mainCamera.WorldToScreenPoint(line.endMarker.position);

        float dist = DistanceBetweenLineSegments(swipeStart, swipeEnd, lineStartScreen, lineEndScreen);
        return dist <= lineTolerancePixels;
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

    private float DistanceBetweenLineSegments(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float d1 = DistancePointToSegment(p1, q1, q2);
        float d2 = DistancePointToSegment(p2, q1, q2);
        float d3 = DistancePointToSegment(q1, p1, p2);
        float d4 = DistancePointToSegment(q2, p1, p2);
        return Mathf.Min(Mathf.Min(d1, d2), Mathf.Min(d3, d4));
    }

    private void PerformChop(CutLine line)
    {
        Debug.Log($"Chopped line between {line.startMarker.name} and {line.endMarker.name}");
        // To-do: sounds, ingredient halves, particles, etc.
    }
}
