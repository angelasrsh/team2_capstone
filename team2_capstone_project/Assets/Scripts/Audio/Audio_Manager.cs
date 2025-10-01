using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimoire
{
  public class Audio_Manager : MonoBehaviour
  {
    public static Audio_Manager instance;

    // --- Sources ---
    private List<AudioSource> sfxSources;
    private int nextSfxIndex = 0;
    private AudioSource ambientSource;
    private AudioSource footstepsSource;
    [SerializeField] private int sfxPoolSize = 8;

    [Header("SFX Clips")]
    public AudioClip pickupSFX;
    public AudioClip clickSFX;
    [SerializeField] private AudioClip goodDishMade;
    public AudioClip textSound;
    public AudioClip menuOpen;
    public AudioClip menuClose;
    public AudioClip bookClose;
    public AudioClip doorBell;
    public AudioClip firstAreaWalkingSFX;
    public AudioClip sparkle;

    [Header("Cauldron Specific")]
    public AudioClip addingOneIngredient;
    public AudioClip addingMultiIngredients;
    public AudioClip addingWater;
    private AudioSource stirringSource;
    private AudioSource bubblingSource;
    private AudioSource fireAmbientSource;
    [SerializeField] private AudioClip stirring;
    [SerializeField] private AudioClip bubbling;
    [SerializeField] private AudioClip startFire;
    [SerializeField] private AudioClip ambientFire;

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

      // --------------------------
      // Build SFX pool dynamically
      // --------------------------
      sfxSources = new List<AudioSource>(sfxPoolSize);

      for (int i = 0; i < sfxPoolSize; i++)
      {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        sfxSources.Add(src);
      }

      // Ambient gets its own dedicated source
      ambientSource = gameObject.AddComponent<AudioSource>();
      ambientSource.loop = true;

      // Dedicated sources for stirring, bubbling, fire
      stirringSource = gameObject.AddComponent<AudioSource>();
      stirringSource.loop = true;

      bubblingSource = gameObject.AddComponent<AudioSource>();
      bubblingSource.loop = true;

      fireAmbientSource = gameObject.AddComponent<AudioSource>();
      fireAmbientSource.loop = true;

      // Dedicated source for footsteps
      footstepsSource = gameObject.AddComponent<AudioSource>();
      footstepsSource.loop = false;
      footstepsSource.playOnAwake = false;
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
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
      if (clip == null) return;

      AudioSource src = sfxSources[nextSfxIndex];
      nextSfxIndex = (nextSfxIndex + 1) % sfxSources.Count;

      src.volume = Mathf.Clamp01(volume);
      src.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
      src.PlayOneShot(clip);
    }

    public void ForestWalk()
    {
      PlaySFX(firstAreaWalkingSFX, 0.1f, Random.Range(0.9f, 1.1f));
    }

    public void StopAllSFX()
    {
      foreach (var src in sfxSources)
        src.Stop();
    }

    public IEnumerator ResetSFXAfterClip(float delay, AudioSource src)
    {
      yield return new WaitForSeconds(delay);

      if (src != null)
      {
        src.volume = 1f;
        src.pitch = 1f;
      }
    }

    // -----------------------------------------------------------------------
    // CAULDRON SPECIFIC METHODS!!
    #region Cauldron SFX
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
      bubblingSource.volume = 0.5f;
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
      PlaySFX(startFire, 0.5f, 1f);
      PlayAmbientFireOnLoop();
    }

    public void PlayAmbientFireOnLoop()
    {
      if (fireAmbientSource == null || ambientFire == null)
        return;

      fireAmbientSource.clip = ambientFire;
      fireAmbientSource.volume = 0.5f;

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

    public void AddOneIngredient()
    {
      float randomPitch = Random.Range(0.8f, 1.2f);
      PlaySFX(addingOneIngredient, 0.5f, randomPitch);
    }

    public void AddWater()
    {
      if (addingWater == null) return;

      PlaySFX(addingWater, 0.3f, 0.8f);
    }
    #endregion

    #region Footsteps SFX
    // -------------------------------------------------------
    // FOOTSTEPS
    public void PlayFootsteps(AudioClip clip, float speed = 1.6f)
    {
      if (clip == null) return;

      if (!footstepsSource.isPlaying)
      {
        footstepsSource.volume = 0.1f;

        // Slightly vary pitch to avoid monotony
        footstepsSource.pitch = Random.Range(speed - 0.1f, speed + 0.1f);
        footstepsSource.PlayOneShot(clip);
      }
    }

    public void StopFootsteps()
    {
      if (footstepsSource.isPlaying)
        footstepsSource.Stop();
    }
    #endregion

    public void PlayDoorbell()
    {
      float randomPitch = Random.Range(0.9f, 1.05f);
      PlaySFX(doorBell, 0.4f, randomPitch);
    }
    
    public void PlaySparkleSFX()
    {
        float[] allowedPitches = { 1.3f, 1.5f, 1.7f };
        float chosenPitch = allowedPitches[Random.Range(0, allowedPitches.Length)];

        PlaySFX(sparkle, 0.3f, chosenPitch);
    }
  }
}

