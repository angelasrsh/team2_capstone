using System.Collections;
using UnityEngine;
using Grimoire;

public class First_Foraging_Area_Walking_SFX : MonoBehaviour
{
    private Player_Controller player;

    [Header("Footstep Clips")]
    public AudioClip leftFootSFX;
    public AudioClip rightFootSFX;

    [Header("Step Timing (seconds)")]
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;

    private float stepTimer = 0f;
    private bool isLeftStep = true;

    private void Start()
    {
        player = FindObjectOfType<Player_Controller>();
    }

    private void Update()
    {
        if (player == null || !player.IsMoving() || !player.controller.isGrounded)
        {
            stepTimer = 0f;
            return;
        }

        // Count down to next step
        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            // Choose clip
            AudioClip stepClip = isLeftStep ? leftFootSFX : rightFootSFX;

            // Slight random pitch variation for realism
            float pitch = Random.Range(0.9f, 1.1f);
            Audio_Manager.instance.PlaySFX(stepClip, 0.15f, pitch);

            // Alternate and reset timer
            isLeftStep = !isLeftStep;
            float interval = player.IsSprinting() ? sprintStepInterval : walkStepInterval;
            interval *= Random.Range(0.9f, 1.1f);

            stepTimer = interval;
        }
    }
}
