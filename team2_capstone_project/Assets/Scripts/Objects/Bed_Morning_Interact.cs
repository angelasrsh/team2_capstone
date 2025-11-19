using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed_Morning_Interact : Object_Dialogue_Interact
{
    public override void PlayDialogInteraction()
    {
        if (Day_Turnover_Manager.Instance == null)
            return;

        if (Day_Turnover_Manager.Instance.currentTimeOfDay != Day_Turnover_Manager.TimeOfDay.Morning)
        {
            Debug.Log("[Bed] Interaction blocked because it is not morning.");
            return;
        }

        base.PlayDialogInteraction();
    }
}

