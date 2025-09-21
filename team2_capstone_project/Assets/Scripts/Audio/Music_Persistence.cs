using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
public class Music_Persistence : MonoBehaviour
{
    public static Music_Persistence instance;
    [HideInInspector] public AudioSource musicSource, ambientSource;   
    private AudioClip currentMusic, currentAmbient;
    
    private Coroutine musicFadeCoroutine;
    private Coroutine ambientFadeCoroutine;

    private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                AudioSource[] sources = GetComponents<AudioSource>();
                if (sources.Length >= 3)
                {
                    // Match order in AudioManager: 0 = music, 1 = sfx, 2 = ambient
                    musicSource = sources[0];
                    ambientSource = sources[2];
                }
                else
                {
                    Debug.LogError("MusicPersistence: You must have at least 3 AudioSources (music, sfx, ambient)");
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

    public void CheckMusic(AudioClip newMusic, float targetVolume)
    {
        if (newMusic != currentMusic)
        {
            currentMusic = newMusic;
            musicSource.clip = newMusic;

            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(MusicFadeIn(musicSource, targetVolume, 1f));
        }
    }

    public void CheckAmbient(AudioClip newAmbient, float targetVolume)
    {
        if (newAmbient != currentAmbient)
        {
            currentAmbient = newAmbient;
            ambientSource.clip = newAmbient;

            if (ambientFadeCoroutine != null)
                StopCoroutine(ambientFadeCoroutine);

            ambientFadeCoroutine = StartCoroutine(AmbientFadeIn(ambientSource, targetVolume, 1f));
        }
    }


    public void PreTransitionCheckMusic(AudioClip newMusic)
    {
        if (newMusic != currentMusic)
        {
            StartCoroutine(MusicFadeOut(musicSource, 1f));
        }
    }

    public void PreTransitionCheckAmbient(AudioClip newAmbient)
    {
        if (newAmbient != currentAmbient)
        {
            StartCoroutine(AmbientFadeOut(ambientSource, 1f));
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
        currentMusic = null;
    }

    public void StopAmbient()
    {
        ambientSource.Stop();
        currentAmbient = null;
    }

    public IEnumerator MusicFadeOut(AudioSource musicSource, float fadeDuration)
    {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    public IEnumerator AmbientFadeOut(AudioSource ambientSource, float fadeDuration)
    {
        float startVolume = ambientSource.volume;

        while (ambientSource.volume > 0)
        {
            ambientSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        ambientSource.Stop();
        ambientSource.volume = startVolume;
    }

    public IEnumerator MusicFadeIn(AudioSource musicSource, float targetVolume, float fadeDuration)
    {
        musicSource.volume = 0f;
        musicSource.Play();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }


    public IEnumerator AmbientFadeIn(AudioSource ambientSource, float targetVolume, float fadeDuration)
    {
        ambientSource.volume = 0f;
        ambientSource.Play();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        ambientSource.volume = targetVolume;
    }
}
}
