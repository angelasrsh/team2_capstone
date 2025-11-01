using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Grimoire
{
    public class Screen_Fade : MonoBehaviour
    {
        public static Screen_Fade instance;
        public CanvasGroup fadeCanvasGroup;
        public float fadeDuration = 1.0f;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Ensure initial alpha state (optional)
            if (fadeCanvasGroup == null)
                fadeCanvasGroup = GetComponent<CanvasGroup>();
        }

        public IEnumerator BlackFadeIn()
        {
            float timer = 0f;
            while (timer <= fadeDuration)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        public IEnumerator BlackFadeOut()
        {
            float timer = 0f;
            while (timer <= fadeDuration)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }
    }
}
