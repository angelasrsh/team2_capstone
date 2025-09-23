using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
public class Audio_Manager : MonoBehaviour
{
    public static Audio_Manager instance;
    private AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip pickupSFX;
    public AudioClip clickSFX;
    public AudioClip textSound;
    public AudioClip menuOpen;
    public AudioClip menuClose;
    public AudioClip bookClose;

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
                // Assign only SFX here; others are used by MusicPersistence
                sfxSource = sources[1];
            }
            else
            {
                Debug.LogError("AudioManager: You must have at least 3 AudioSources (music, sfx, ambient)");
            }
        }

    // Music
    public void PlayMusic(AudioClip clip) => Music_Persistence.instance?.CheckMusic(clip, 1f);
    public void StopMusic() => Music_Persistence.instance?.StopMusic();
    public void SetMusicVolume(float volume)
    {
        if (Music_Persistence.instance?.musicSource != null)
            Music_Persistence.instance.musicSource.volume = Mathf.Clamp01(volume);
    }

    // Ambient
    public void PlayAmbientLoop(AudioClip clip) => Music_Persistence.instance?.CheckAmbient(clip, 1f);
    public void StopAmbient() => Music_Persistence.instance?.StopAmbient();
    public void SetAmbientVolume(float volume)
    {
        if (Music_Persistence.instance?.ambientSource != null)
            Music_Persistence.instance.ambientSource.volume = Mathf.Clamp01(volume);
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
