using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


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
    public GameObject ingredient;  
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
    public Knife_Script knife_script;
    
    [Header("Cut Settings")]
    public GameObject cutPrefab;
    public GameObject cutImagePrefab; // Prefab for the cut versions of the image

    public Ingredient_Data ingredient_data_var;
    public GameObject ingredient_object;
    private bool hasIngredientData = true;


    public Ingredient_Data SetIngredientData(Ingredient_Data ingredientData, GameObject ing_gameOb)
    {
        //this function is used to get the ingredientData from DragAll
        ingredient_data_var = ingredientData;
        ingredient_object = ing_gameOb;
        hasIngredientData = true;

        // Debug.Log("[Chp_ctrller] Received ingredient data: " + ingredient_data_var); //works
        return ingredient_data_var;
    }
    private void showCuttingLines()
    {
        //function to make the lines appear
        // then start the game


    }

    public Ingredient_Data defaultIngredient; // Assign in Inspector

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
      
        knife_script = FindObjectOfType<Knife_Script>();
        if (knife_script == null)
        {
            Debug.LogError("knife_Script not found in CHop_Controller!");
        }
        // Debug.Log("[Chp_Cntrller] ingredient_data_var = " + ingredient_data_var);
        // Start with script disabled
        // DeactivateChopController();

    }

    public void DeactivateChopController()
    {
        
    }
    private Vector2 swipeStart;

    private Vector3 knife_pos;
    public bool knife_is_overlapping = false;
    public RectTransform redZoneForKnife;
    public bool in_inventory = false;
    void Update()
    {
        // if (drag_all_script.canDrag == false) //TODO check if this is set up properly in drag all
        {

            if (hasIngredientData)
            {
                GameObject red_zone_found = GameObject.Find("RedZoneForKnife");
                if (red_zone_found != null)
                    redZoneForKnife = red_zone_found.GetComponent<RectTransform>();
                else
                {
                    Debug.Log("[Chp_contrller] Could not find redZone4Knife!");
                }
                knife_pos = knife_script.currKnifePosition;

                if (!knife_is_overlapping && Drag_All.IsOverlapping(knife_script.knifeRectTransform, redZoneForKnife))
                {
                    Debug.Log("[Chp_cntrller] Knife is overlapping redZoneForKnife");
                    // SpawnCutPrefabs();
                    knife_is_overlapping = true;
                }
                else
                {
                    knife_is_overlapping = false;

                }

                if (knife_is_overlapping == true)
                {
                    Debug.Log("went inside the chopscript.knife_is_overlappting if statement");

                    // Get the Image component from the transform, then change the sprite
                    Image imageComponent = ingredient_object.GetComponent<Image>();

                    if (imageComponent != null)
                    {
                        // Use a cut image if available
                        if (ingredient_data_var.CutIngredientImages.Length > 0)
                        {
                            imageComponent.sprite = ingredient_data_var.CutIngredientImages[0];
                        }
                        //use the regular image
                        else
                        {
                            imageComponent.sprite = ingredient_data_var.Image;
                        }
                    }
                    if (Ingredient_Inventory.Instance.HasItem(ingredient_data_var.makesIngredient[0].ingredient) == false)
                    {
                        in_inventory = false;
                    }
                    if (!in_inventory)
                    {
                        Ingredient_Inventory.Instance.AddResources(ingredient_data_var.makesIngredient[0].ingredient, 1);

                        StartCoroutine(DelayedActions());

                        IEnumerator DelayedActions()
                        {
                            yield return new WaitForSeconds(0.75f); // Wait .75 seconds

                            imageComponent.enabled = false;
                            Drag_All.cuttingBoardActive = false;
                        }
                        in_inventory = true;
                    }
                }
            }
            // Debug.Log("[Chop_cntrller] position of knife" + position);
            // if (Input.GetMouseButtonDown(0))
            // {
            //     isDragging = true;
            //     swipeStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //     startDragPos = Mouse.current.position.ReadValue();
            //     startTime = Time.time;
            // }
            // else if (isDragging && Input.GetMouseButtonUp(0))
            // {
            //     isDragging = false;
            //     Vector2 endDragPos = Mouse.current.position.ReadValue();
            //     // float duration = Time.time - startTime;
            //     // SpawnCut();

            //     // EvaluateChop(startDragPos, endDragPos, duration);
            // }
        }
        
    }


    private void SpawnCutPrefabs()
    {
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
    
        ingredient_data_var.Image = ingredient_data_var.CutIngredientImages[0];
        
        // Vector2 swipeEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition); //where the swipe ended
        // GameObject cutInstance = Instantiate(cutPrefab, swipeStart, Quaternion.identity); //create the cut object
        // cutInstance.GetComponent<LineRenderer>().SetPosition(0, swipeStart); //create the line
        // cutInstance.GetComponent<LineRenderer>().SetPosition(1, swipeEnd);

        // Vector2[] colliderPoints = new Vector2[2];
        // colliderPoints[0] = Vector2.zero;
        // colliderPoints[1] = swipeEnd - swipeStart;

        // Destroy(cutInstance);
        Debug.Log("[SpawnCut] done.");

    }

    private void EvaluateChop(Vector2 startScreen, Vector2 endScreen, float duration)
    {
        Vector2 chop = endScreen - startScreen;
        float distance = chop.magnitude;

        if (distance < minChopDistance) { Debug.Log("Chop too short"); return; }
        if (duration <= 0f) duration = 0.0001f;
        float speed = distance / duration;
        if (speed < minChopSpeed) { Debug.Log("Chop too slow"); return; }

        Vector2 chopDir = chop.normalized;
        float alignment = Vector2.Dot(chopDir, requiredDirection.normalized);
        if (alignment < directionTolerance) { Debug.Log("Wrong direction"); return; }

        if (requireSequential)
        {
            // find first uncut line
            for (int i = 0; i < cutLines.Length; i++)
            {
                if (!cutLines[i].cutCompleted)
                {
                    if (IsChopNearLine_ScreenSpace(startScreen, endScreen, cutLines[i]))
                    {
                        cutLines[i].cutCompleted = true;
                        PerformChop(cutLines[i]);
                    }
                    else
                    {
                        Debug.Log("Missed current required line");
                    }
                    return; // only try the next uncut line
                }
            }
            Debug.Log("All lines already cut");
        }
        else
        {
            // allow any order
            foreach (var line in cutLines)
            {
                if (line.cutCompleted) continue;
                if (IsChopNearLine_ScreenSpace(startScreen, endScreen, line))
                {
                    line.cutCompleted = true;
                    PerformChop(line);
                    return;
                }
            }
            Debug.Log("Chop didn't match any line");
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

    private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
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
