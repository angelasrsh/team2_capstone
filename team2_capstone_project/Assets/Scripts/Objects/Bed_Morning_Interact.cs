using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed_Morning_Interact : Object_Dialogue_Interact
{
    private void CheckIfMorning()
    {
        if (Day_Turnover_Manager.Instance == null) return;

        var tod = Day_Turnover_Manager.Instance.currentTimeOfDay;
        if (tod != Day_Turnover_Manager.TimeOfDay.Morning)
        {
            this.enabled = false;
            return;
        }

        // Play morning dialogue
        PlayDialogInteraction();
    }
}
