using System.Collections;
using UnityEngine;
using Grimoire;

public class First_Foraging_Area_Walking_SFX : MonoBehaviour
{
    private Player_Controller player;
    private bool playLeft = true;
    private float stepTimer = 0f;
    public float baseStepRate = 0.5f;

    private void Start()
    {
        player = FindObjectOfType<Player_Controller>();
    }

    private void Update()
    {
        if (player == null) return;

        if (player.IsMoving() && player.controller.isGrounded)
        {
            float stepInterval = baseStepRate / (player.isSprinting ? player.sprintMultiplier : 1f);
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
    }

    private void PlayFootstep()
    {
        AudioClip clip = null;

        switch (player.currentSurface)
        {
            case "Wood":
                clip = playLeft ? Audio_Manager.instance.woodLeftFootstep : Audio_Manager.instance.woodRightFootstep;
                break;
            case "Stone":
                clip = playLeft ? Audio_Manager.instance.stoneLeftFootstep : Audio_Manager.instance.stoneRightFootstep;
                break;
            default:
                clip = playLeft ? Audio_Manager.instance.grassLeftFootstep : Audio_Manager.instance.grassRightFootstep;
                break;
        }

        Audio_Manager.instance.PlaySFX(clip, 0.15f, Random.Range(0.9f, 1.1f));
        playLeft = !playLeft;
    }
}
