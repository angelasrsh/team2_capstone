using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;

public class First_Foraging_Area_Walking_SFX : MonoBehaviour
{
    private Player_Controller player;
    private void Start()
    {
        player = FindObjectOfType<Player_Controller>();
    }

    private void Update()
    {
        if (player != null && player.IsMoving())
        {
            Audio_Manager.instance.PlayFootsteps(Audio_Manager.instance.firstAreaWalkingSFX);
        }
        else if (player != null && !player.IsMoving())
        {
            Audio_Manager.instance.StopFootsteps();
        }
    }
}

