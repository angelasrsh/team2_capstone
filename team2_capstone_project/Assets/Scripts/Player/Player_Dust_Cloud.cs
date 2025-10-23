using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player_Dust_Cloud : MonoBehaviour
{
    public GameObject dustPrefab;
    public Transform dustSpawnPoint;
    public float spawnInterval = 0.2f;
    public float minSpeed = 0.1f;

    private CharacterController cc;
    private Player_Controller player;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        player = GetComponent<Player_Controller>();
    }

    private void Update()
    {
        if (player == null) return;

        bool grounded = cc.isGrounded;
        float speed = player.movement.magnitude;
        Vector3 moveDir = new Vector3(-player.movement.x - 0.25f, 0f, player.movement.y + 2f);

        // Flip spawn point based on movement direction
        if (moveDir.x != 0)
        {
            Vector3 localPos = dustSpawnPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * Mathf.Sign(moveDir.x);
            dustSpawnPoint.localPosition = localPos;
        }

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


    private void SpawnDust()
    {
        if (dustPrefab == null || dustSpawnPoint == null) return;

        GameObject dust = Instantiate(dustPrefab, dustSpawnPoint.position, Quaternion.identity);
        dust.transform.localScale *= Random.Range(0.8f, 1.2f);
    }
}
