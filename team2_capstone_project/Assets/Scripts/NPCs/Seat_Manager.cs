using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat_Manager : MonoBehaviour
{
    public static Seat_Manager Instance;

    private List<Transform> allSeats = new List<Transform>();
    private HashSet<Transform> occupiedSeats = new HashSet<Transform>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Collect all seats that are children of this object
        foreach (Transform seat in transform)
        {
            allSeats.Add(seat);
        }
    }

    public Transform GetRandomAvailableSeat()
    {
        List<Transform> freeSeats = allSeats.FindAll(seat => !occupiedSeats.Contains(seat));
        if (freeSeats.Count == 0) return null;

        Transform chosen = freeSeats[Random.Range(0, freeSeats.Count)];
        occupiedSeats.Add(chosen);
        return chosen;
    }

    public void FreeSeat(Transform seat)
    {
        if (occupiedSeats.Contains(seat))
            occupiedSeats.Remove(seat);
    }

    public void AddSeat(Transform newSeat)
    {
        if (!allSeats.Contains(newSeat))
            allSeats.Add(newSeat);
    }

    public Transform GetSeatByIndex(int index)
    {
        if (index >= 0 && index < allSeats.Count) return allSeats[index];
        return null;
    }

    public int GetSeatIndex(Transform seat)
    {
        return allSeats.IndexOf(seat);
    }
}

