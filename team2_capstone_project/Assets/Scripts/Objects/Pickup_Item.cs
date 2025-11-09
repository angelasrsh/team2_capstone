using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Grimoire;

[RequireComponent(typeof(Rigidbody))]
public class Pickup_Item : MonoBehaviour
{
    private Ingredient_Data data;
    private Transform player;
    private Rigidbody rb;
    private SpriteRenderer sr;

    [Header("Physics/Attraction")]
    [SerializeField] private float attractForce = 35f;
    [SerializeField] private float maxAttractSpeed = 8f;
    [SerializeField] private float attractionRange = 10f;
    [SerializeField] private float collectRangeXZ = 0.6f;
    [SerializeField] private float pickupDelay = 0.45f;
    [SerializeField] private float minLifetime = 0.12f;

    [Header("Spawn Arc Motion")]
    [SerializeField] private float arcHeight = 1.2f;
    [SerializeField] private float arcDuration = 0.4f;
    [SerializeField] private float arcDistance = 0.3f;

    [Header("Hover Settings")]
    [SerializeField] private float hoverAmplitude = 0.06f;
    [SerializeField] private float hoverFrequency = 3f;

    private bool canAttract = false;
    private bool canCollect = false;
    private bool collected = false;
    private float lifeTimer = 0f;
    private bool hasSettled = false;
    private Vector3 baseLocalSpriteOffset;

    /// <summary>
    /// Initialize the pickup item with the given ingredient data.
    /// </summary>
    /// <param name="ingredient"></param>
    public void Initialize(Ingredient_Data ingredient)
    {
        data = ingredient;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (data?.Image != null)
            sr.sprite = data.Image;

        baseLocalSpriteOffset = sr.transform.localPosition;
        sr.transform.localRotation = Quaternion.identity;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.WakeUp();

        StartCoroutine(SpawnArcMotion());
        StartCoroutine(EnablePickupPhases());
    }

    /// <summary>
    /// Enable attraction and collection after initial delay.
    /// </summary>
    private IEnumerator EnablePickupPhases()
    {
        yield return new WaitForSeconds(pickupDelay);
        canAttract = true;

        yield return new WaitForSeconds(0.2f);
        canCollect = true;
    }

    /// <summary>
    /// Handle attraction to player and collection when in range.
    /// </summary>
    private void FixedUpdate()
    {
        if (!hasSettled || !canAttract || player == null || collected)
            return;

        lifeTimer += Time.fixedDeltaTime;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 toPlayerXZ = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float distXZ = toPlayerXZ.magnitude;

        if (distXZ <= attractionRange)
        {
            rb.WakeUp();  // Ensure it's not sleeping

            Vector3 dir = toPlayer.normalized;
            dir.y = Mathf.Clamp01(dir.y + 0.35f); // slight lift toward player
            float distFactor = Mathf.Clamp01((attractionRange - distXZ) / attractionRange);
            float applied = attractForce * (0.5f + 1.2f * distFactor);

            rb.AddForce(dir * applied, ForceMode.Acceleration);

            // Clamp horizontal speed
            Vector3 v = rb.velocity;
            Vector3 horiz = new Vector3(v.x, 0f, v.z);
            if (horiz.magnitude > maxAttractSpeed)
                rb.velocity = new Vector3(horiz.normalized.x * maxAttractSpeed, v.y, horiz.normalized.z * maxAttractSpeed);
        }

        if (canCollect && lifeTimer > minLifetime && distXZ <= collectRangeXZ)
            Collect();
    }

    private void Update()
    {
        float hover = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        sr.transform.localPosition = baseLocalSpriteOffset + new Vector3(0f, hover, 0f);
    }

    private IEnumerator SpawnArcMotion()
    {
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector3 start = transform.position;
        Vector2 randomXZ = Random.insideUnitCircle * arcDistance;
        Vector3 end = start + new Vector3(randomXZ.x, 0f, randomXZ.y);

        float timer = 0f;
        while (timer < arcDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / arcDuration);
            float parabola = 4f * (t - t * t);
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += parabola * arcHeight;
            transform.position = pos;
            yield return null;
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        hasSettled = true;
    }

    private void Collect()
    {
        if (collected) return;
        collected = true;

        Ingredient_Inventory.Instance.AddResources(data, 1);
        Save_Manager.instance.AutoSave();
        Audio_Manager.instance.PlaySFX(Audio_Manager.instance.pickupSFX, 0.6f, 1f);

        sr.enabled = false;
        Destroy(gameObject, 0.05f);
    }
}
