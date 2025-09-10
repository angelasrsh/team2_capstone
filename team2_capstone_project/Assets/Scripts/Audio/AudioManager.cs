using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource sfxSource;

    [Header("Default Audio Clips")]
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private AudioClip defaultAmbient;

    private void Awake()
        {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Get AudioSources in order: 0 = music, 1 = sfx, 2 = ambient
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 3)
        {
            // Assign only SFX here; others are used by MusicPersistance
            sfxSource = sources[1];
        }
        else
        {
            Debug.LogError("AudioManager: You must have at least 3 AudioSources (music, sfx, ambient)");
        }

        StartCoroutine(PlayDefaultAudioDelayed());
    }

    private IEnumerator PlayDefaultAudioDelayed()
    {
         yield return new WaitForSeconds(0.1f);
        if (defaultMusic != null) PlayMusic(defaultMusic);
        if (defaultAmbient != null) PlayAmbientLoop(defaultAmbient);
    }

    // Music
    public void PlayMusic(AudioClip clip) => MusicPersistence.instance?.CheckMusic(clip);
    public void StopMusic() => MusicPersistence.instance?.StopMusic();
    public void SetMusicVolume(float volume)
    {
        if (MusicPersistence.instance?.musicSource != null)
            MusicPersistence.instance.musicSource.volume = Mathf.Clamp01(volume);
    }

    // Ambient
    public void PlayAmbientLoop(AudioClip clip) => MusicPersistence.instance?.CheckAmbient(clip);
    public void StopAmbient() => MusicPersistence.instance?.StopAmbient();
    public void SetAmbientVolume(float volume)
    {
        if (MusicPersistence.instance?.ambientSource != null)
            MusicPersistence.instance.ambientSource.volume = Mathf.Clamp01(volume);
    }

    // SFX
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetSFXVolume(float volume) => sfxSource.volume = Mathf.Clamp01(volume);
    public void SetSFXPitch(float pitch) => sfxSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
    public void StopSFX() => sfxSource?.Stop();
    }
}
