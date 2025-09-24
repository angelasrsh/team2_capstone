using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
  public class Audio_Manager : MonoBehaviour
  {
    public static Audio_Manager instance;
    private AudioSource sfxSource;
    private AudioSource ambientSource;

    [Header("SFX Clips")]
    public AudioClip pickupSFX;
    public AudioClip clickSFX;
    [SerializeField] private AudioClip goodDishMade;

    [Header("Cauldron Specific")]
    private AudioSource stirringSource;
    private AudioSource bubblingSource;
    private AudioSource fireAmbientSource;
    [SerializeField] private AudioClip stirring;
    [SerializeField] private AudioClip bubbling;
    [SerializeField] private AudioClip startFire;
    [SerializeField] private AudioClip ambientFire;
    [SerializeField] private AudioClip addingOneIng;
    [SerializeField] private AudioClip addingMultiIng;

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
        // Assign only SFX and ambient (for cauldron) here; others are used by MusicPersistance
        sfxSource = sources[1];
        ambientSource = sources[2];
      }
      else
        Debug.LogError("AudioManager: You must have at least 3 AudioSources (music, sfx, ambient)");

      // Create dedicated sources for stirring & bubbling
      stirringSource = gameObject.AddComponent<AudioSource>();
      stirringSource.loop = true;

      bubblingSource = gameObject.AddComponent<AudioSource>();
      bubblingSource.loop = true;

      fireAmbientSource = gameObject.AddComponent<AudioSource>();
      fireAmbientSource.loop = true;
    }

    // -----------------------------------------------------------------------
    // Music
    public void PlayMusic(AudioClip clip) => Music_Persistence.instance?.CheckMusic(clip, 1f);
    public void StopMusic() => Music_Persistence.instance?.StopMusic();
    public void SetMusicVolume(float volume)
    {
      if (Music_Persistence.instance?.musicSource != null)
        Music_Persistence.instance.musicSource.volume = Mathf.Clamp01(volume);
    }

    // -----------------------------------------------------------------------
    // Ambient
    public void PlayAmbientLoop(AudioClip clip) => Music_Persistence.instance?.CheckAmbient(clip, 1f);
    public void StopAmbient() => Music_Persistence.instance?.StopAmbient();
    public void SetAmbientVolume(float volume)
    {
      if (Music_Persistence.instance?.ambientSource != null)
        Music_Persistence.instance.ambientSource.volume = Mathf.Clamp01(volume);
    }

    // -----------------------------------------------------------------------
    // SFX
    public void PlaySFX(AudioClip clip)
    {
      if (sfxSource == null || clip == null) return;
      sfxSource.PlayOneShot(clip);
    }

    public void SetSFXVolume(float volume) => sfxSource.volume = Mathf.Clamp01(volume);
    public void SetSFXPitch(float pitch) => sfxSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
    public void StopSFX() => sfxSource?.Stop();

    // -----------------------------------------------------------------------
    // CAULDRON SPECIFIC METHODS!!

    /// <summary>
    /// This method is meant to be used in the minigames where the restaurant music
    /// needs to be lower in volume for other minigame sfx and ambient sounds to be heard.
    /// In Audio_Manager, but used in Drag_All. NOT WORKING IDK WHY
    /// </summary>
    public void LowerRestaurantMusic()
    {
      // SetMusicVolume(0.05f);
      Debug.Log("lowering music!!!!!!!!!!!!!!!");
      Music_Persistence.instance.musicSource.volume = Mathf.Clamp01(0.1f);
    }
    public void GoodDishMade() => PlaySFX(goodDishMade);
    public void PlayStirringOnLoop()
    {
      if (stirring == null || stirringSource == null)
        return;

      stirringSource.clip = stirring;
      if (stirringSource.time > 0f && !stirringSource.isPlaying) // Resume if it was paused
        stirringSource.UnPause();
      else if (!stirringSource.isPlaying) // First play
        stirringSource.Play();
    }
    public void StopStirring()
    {
      if (stirringSource != null && stirringSource.isPlaying)
        stirringSource.Pause();
    }

    public void PlayBubblingOnLoop()
    {
      if (bubbling == null || bubblingSource == null)
        return;

      bubblingSource.clip = bubbling;
      if (!bubblingSource.isPlaying)
        bubblingSource.Play();
    }

    public void StopBubbling()
    {
      if (bubblingSource != null && bubblingSource.isPlaying)
        bubblingSource.Stop(); // restarts bubbling clip to 0 for next play
    }

    public void StartFire()
    {
      PlaySFX(startFire);
      PlayAmbientFireOnLoop();
    }

    public void PlayAmbientFireOnLoop()
    {
      if (fireAmbientSource == null || ambientFire == null)
        return;

      fireAmbientSource.clip = ambientFire;
      fireAmbientSource.volume = 0.2f;

      if (!fireAmbientSource.isPlaying) // Play if not already playing
      {
        fireAmbientSource.Play();
        Debug.Log("[Audio_Manager] Playing ambientFire on loop!");
      }
    }

    public void StopAmbientFire()
    {
      if (fireAmbientSource != null && fireAmbientSource.isPlaying)
        fireAmbientSource.Stop();
    }
    public void addingOneIngredient() => PlaySFX(addingOneIng);
    public void AddingMultiIng() => PlaySFX(addingMultiIng);
  }  
}
