using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Grimoire;

[RequireComponent(typeof(AudioSource))]
public class Dish_Sound_Handler : MonoBehaviour
{
    [Header("Dish Loop Settings")]
    [SerializeField] private AudioClip dishLoop;
    [SerializeField] private float targetVolume = 0.6f;
    [SerializeField] private float fadeDuration = 1f;

    private AudioSource dishSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        dishSource = GetComponent<AudioSource>();
        dishSource.loop = true;
        dishSource.playOnAwake = false;

        // Route through AudioManager's ambientGroup
        if (Audio_Manager.instance != null)
            dishSource.outputAudioMixerGroup = Audio_Manager.instance.ambientGroup;
    }

    private void OnEnable() => Customer_Spawner.OnCustomerCountChanged += HandleCustomerCountChanged;

    private void OnDisable()
    {
        Customer_Spawner.OnCustomerCountChanged -= HandleCustomerCountChanged;
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // Fade out cleanly if object is disabled
        if (gameObject.activeInHierarchy)
            fadeCoroutine = StartCoroutine(FadeOut());
    }

    private void Start()
    {
        int currentCount = FindObjectsOfType<Customer_Controller>().Length;
        HandleCustomerCountChanged(currentCount);
    }

    private void HandleCustomerCountChanged(int count)
    {
        if (count > 0)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        else
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeIn()
    {
        if (dishLoop == null) yield break;

        if (dishSource.clip != dishLoop)
            dishSource.clip = dishLoop;

        if (!dishSource.isPlaying)
            dishSource.Play();

        float startVol = dishSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            dishSource.volume = Mathf.Lerp(startVol, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        dishSource.volume = targetVolume;
    }

    private IEnumerator FadeOut()
    {
        float startVol = dishSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            dishSource.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeDuration);
            yield return null;
        }

        dishSource.volume = 0f;
        dishSource.Stop();
    }
}
