using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Grimoire
{
public class Screen_Fade : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup; 
    public float fadeDuration = 3.0f;

    public void Start() {}

    public IEnumerator BlackFadeIn()
    {
        float timer = 0;
        while (timer <= fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;
    }

    public IEnumerator BlackFadeOut()
    {
        float timer = 0;
        while (timer <= fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;
    }
}
}

