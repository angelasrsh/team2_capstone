using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grimoire;

[System.Serializable]
public class AmbientClipEntry
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.5f;
}

public class Dynamic_Music_Cycling : MonoBehaviour
{
    [Header("Main Music Settings")]
    public AudioClip mainMusic;
    [Range(0f, 1f)] public float musicVolume = 1f;

    [Header("Music Timing")]
    public Vector2 musicPlayDurationRange = new Vector2(60f, 120f); // min/max how long music plays
    public Vector2 silenceDurationRange = new Vector2(20f, 40f); // min/max silence length

    [Header("Optional Ambient Pool During Silence")]
    public bool playExtraAmbientDuringSilence = false;
    public List<AmbientClipEntry> extraAmbientClips = new List<AmbientClipEntry>();

    [Header("Fade Settings")]
    public float fadeDuration = 2f;

    private Coroutine cycleCoroutine;
    private bool isCycling = false;
    private AudioClip lastPlayedAmbient;
    private AudioSource tempAmbientSource;

    private void OnDestroy()
    {
        if (cycleCoroutine != null)
            StopCoroutine(cycleCoroutine);

        if (tempAmbientSource != null)
            Destroy(tempAmbientSource);
    }
    
    private void Start()
    {
        // Create a temporary ambient source just for cycling
        tempAmbientSource = gameObject.AddComponent<AudioSource>();
        tempAmbientSource.playOnAwake = false;
        tempAmbientSource.loop = false;

        // Start main music immediately 
        if (Music_Persistence.instance.musicSource.clip != mainMusic)
        {
            Audio_Manager.instance.PlayMusic(mainMusic);
            Audio_Manager.instance.SetMusicVolume(musicVolume);
        }

        // Begin the looping cycle
        if (!isCycling)
        {
            isCycling = true;
            cycleCoroutine = StartCoroutine(MusicCycleLoop());
        }
    }

    private IEnumerator MusicCycleLoop()
    {
        while (true)
        {
            // 1. Let the music play for a random duration
            float playTime = Random.Range(musicPlayDurationRange.x, musicPlayDurationRange.y);
            yield return new WaitForSeconds(playTime);

            // 2. Fade out the music
            yield return StartCoroutine(Music_Persistence.instance.MusicFadeOut(
                Music_Persistence.instance.musicSource, fadeDuration));

            // 3. Optionally play an ambient clip during the silence
            AmbientClipEntry chosenAmbient = null;

            if (playExtraAmbientDuringSilence && extraAmbientClips.Count > 0)
            {
                chosenAmbient = GetRandomAmbientClip();

                if (chosenAmbient != null && chosenAmbient.clip != null)
                {
                    if (tempAmbientSource != null)
                    {
                        tempAmbientSource.clip = chosenAmbient.clip;
                        tempAmbientSource.volume = 0f;
                        tempAmbientSource.Play();
                        yield return StartCoroutine(FadeVolume(tempAmbientSource, chosenAmbient.volume, fadeDuration));
                    }

                    yield return StartCoroutine(FadeVolume(tempAmbientSource, chosenAmbient.volume, fadeDuration));
                }
            }

            // 4. Let silence/ambient play for a random duration
            float silenceTime = Random.Range(silenceDurationRange.x, silenceDurationRange.y);
            yield return new WaitForSeconds(silenceTime);

            // 5. Fade out the extra ambient (if playing)
            if (chosenAmbient != null && Music_Persistence.instance.ambientSource.isPlaying)
            {
                yield return StartCoroutine(FadeVolume(Music_Persistence.instance.ambientSource, 0f, fadeDuration));
                Music_Persistence.instance.ambientSource.Stop();
            }

            // 6. Fade the main music back in
            yield return StartCoroutine(Music_Persistence.instance.MusicFadeIn(
                Music_Persistence.instance.musicSource, musicVolume, fadeDuration));
        }
    }

    /// <summary>
    /// Selects a random ambient clip entry from the pool, avoiding repetition if possible.
    /// </summary>
    private AmbientClipEntry GetRandomAmbientClip()
    {
        if (extraAmbientClips.Count == 0)
            return null;

        if (extraAmbientClips.Count == 1)
            return extraAmbientClips[0];

        AmbientClipEntry chosen = null;
        int safety = 0;
        do
        {
            chosen = extraAmbientClips[Random.Range(0, extraAmbientClips.Count)];
            safety++;
        }
        while (chosen.clip == lastPlayedAmbient && safety < 10);

        lastPlayedAmbient = chosen.clip;
        Debug.Log($"Chosen ambient clip: {chosen.clip.name}");
        return chosen;
    }

    private IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }
}
