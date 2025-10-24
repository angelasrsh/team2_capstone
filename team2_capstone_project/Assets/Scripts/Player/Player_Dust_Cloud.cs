using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player_Dust_Cloud : MonoBehaviour
{
    [Header("Dust Settings")]
    public GameObject dustPrefab;
    public Transform dustSpawnPoint;
    public float spawnInterval = 0.2f;
    public float minSpeed = 0.1f;

    [Header("General Offsets")]
    public float horizontalOffset = 0.3f;
    public float baseVerticalOffset = 0.05f;

    [Header("Directional Extra Offsets")]
    public Vector3 upOffset = new Vector3(0f, -0.05f, -0.05f);
    public Vector3 downOffset = new Vector3(0f, 0.05f, 0.05f);
    [Tooltip("Extra offset for left/right movement (applied on top of horizontalOffset).")]
    public Vector3 horizontalExtraOffset = new Vector3(0f, 0f, -0.05f);

    private CharacterController cc;
    private Player_Controller player;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        player = GetComponent<Player_Controller>();
    }

    void Update()
    {
        if (!player || !dustSpawnPoint || !dustPrefab)
            return;

        bool grounded = cc.isGrounded;
        float speed = player.movement.magnitude;

        if (speed > 0.01f)
        {
            Vector3 localPos = Vector3.zero;

            float moveX = player.movement.x;
            float moveY = player.movement.y;

            // Base position
            localPos.y = baseVerticalOffset;

            // Determine which direction we’re moving
            if (Mathf.Abs(moveY) > Mathf.Abs(moveX))
            {
                // Vertical movement (Up/Down)
                if (moveY > 0f)
                    localPos += upOffset;
                else
                    localPos += downOffset;
            }
            else
            {
                // Horizontal movement (Left/Right)
                localPos.x = horizontalOffset * Mathf.Sign(moveX);
                localPos += horizontalExtraOffset;
            }

            dustSpawnPoint.localPosition = localPos;
        }

        // Spawn logic
        if (grounded && speed > minSpeed)
        {
            if (!IsInvoking(nameof(SpawnDust)))
                InvokeRepeating(nameof(SpawnDust), 0f, spawnInterval);
        }
        else
        {
            CancelInvoke(nameof(SpawnDust));
        }
    }

    void SpawnDust()
    {
        GameObject dust = Instantiate(dustPrefab, dustSpawnPoint.position, Quaternion.identity);
        dust.transform.localScale *= Random.Range(0.8f, 1.2f);
    }
}
