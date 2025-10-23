using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dust_Cloud_Fade : MonoBehaviour
{
    public float lifeTime = 0.5f;
    private float timer;
    private SpriteRenderer sr;
    private Vector3 startScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifeTime;
        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - t);

        if (timer >= lifeTime)
            Destroy(gameObject);
    }
}
