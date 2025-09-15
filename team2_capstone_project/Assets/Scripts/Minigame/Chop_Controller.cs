using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private void showCuttingLines()
    {
        //function to make the lines appear
        // then start the game


    }
    void Start()
    {
            // Start with script disabled
        DeactivateChopController();
    }

    public void DeactivateChopController()
    {
        
    }
    private Vector2 swipeStart;
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            swipeStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startDragPos = Mouse.current.position.ReadValue();
            startTime = Time.time;
        }
        else if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            Vector2 endDragPos = Mouse.current.position.ReadValue();

            float duration = Time.time - startTime;

            EvaluateChop(startDragPos, endDragPos, duration);
            SpawnCut();
        }
    }
    [Header("Cut Settings")]
    public GameObject cutPrefab;
    public GameObject cutImagePrefab; // Prefab for the cut versions of the image

    private void SpawnCut()
    {
        Vector2 swipeEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition); //where the swipe ended
        GameObject cutInstance = Instantiate(cutPrefab, swipeStart, Quaternion.identity); //create the cut object
        cutInstance.GetComponent<LineRenderer>().SetPosition(0, swipeStart); //create the line
        cutInstance.GetComponent<LineRenderer>().SetPosition(1, swipeEnd);


        Vector2[] colliderPoints = new Vector2[2];
        colliderPoints[0] = Vector2.zero;
        colliderPoints[1] = swipeEnd - swipeStart;


        Destroy(cutInstance);
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
