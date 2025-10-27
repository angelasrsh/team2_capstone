using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
    [RequireComponent(typeof(Collectible_Object))]
    public class Timed_Collectible : MonoBehaviour
    {
        [Header("Timed Collectible Settings")]
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private bool blinkBeforeDestroy = true;
        [SerializeField] private bool explodeOnDestroy = true;
        [SerializeField] private Color blinkColor = Color.red;
        [SerializeField] private float finalBlinkSpeed = 10f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        // private bool isCollected = false;

        private void Awake()
        {
            if (targetSpriteRenderer != null)
                spriteRenderer = targetSpriteRenderer;
            else
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            StartCoroutine(LifetimeRoutine());
        }

        private IEnumerator LifetimeRoutine()
        {
            float elapsed = 0f;

            while (elapsed < lifetime)
            {
                // Stop if this collectible gets picked up (Collectible_Object disables itself)
                if (!enabled || !gameObject.activeInHierarchy)
                    yield break;

                elapsed += Time.deltaTime;

                if (blinkBeforeDestroy && spriteRenderer != null)
                {
                    float blinkSpeed = Mathf.Lerp(2f, finalBlinkSpeed, elapsed / lifetime);
                    float t = Mathf.PingPong(Time.time * blinkSpeed, 0.5f);
                    spriteRenderer.color = Color.Lerp(originalColor, blinkColor, t);
                }

                yield return null;
            }

            // Restore color before destroying
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            if (explodeOnDestroy)
            {
                // Particle effect on destroy
                var ps = GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                    ps.Play();

                yield return new WaitForSeconds(0.2f);
            }

            Destroy(gameObject);
        }
    }
}
