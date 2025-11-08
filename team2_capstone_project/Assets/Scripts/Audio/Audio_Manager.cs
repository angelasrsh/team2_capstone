using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    [Header("Audio Mixer Routing")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    public AudioMixerGroup ambientGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup footstepsGroup;

    [Header("SFX Clips")]
    public AudioClip pickupSFX;
    public AudioClip clickSFX;
    [SerializeField] private AudioClip goodDishMade;
    public AudioClip textSound;
    public AudioClip menuOpen;
    public AudioClip menuClose;
    public AudioClip bookClose;
    public AudioClip doorBell;
    public AudioClip sparkle;
    public AudioClip errorBuzz;
    public AudioClip bagPutIn;
    public AudioClip bagOpen;
    public AudioClip bagClose;
    public AudioClip pageTurn;
    public AudioClip bookOpen;
    public AudioClip goToBed;
    public AudioClip getUpFromBed;
    public AudioClip openDoor;
    public AudioClip closeDoor;
    public AudioClip lockedGateOpen;
    public AudioClip pageFlip;
    public AudioClip journalTabSelect;
    public AudioClip teleport;
    public AudioClip orderServed;
    public AudioClip openShopSFX;
    public AudioClip closeShopSFX;
    public AudioClip pianoHit;
    public AudioClip lampSwitch;

    [Header("Footstep Clips")]
    public AudioClip grassLeftFootstep;
    public AudioClip grassRightFootstep;
    public AudioClip woodLeftFootstep;
    public AudioClip woodRightFootstep;
    public AudioClip stoneLeftFootstep;
    public AudioClip stoneRightFootstep;

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

    private AudioSource dishLoopSource;
    [SerializeField] private AudioClip dishLoopClip;

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
        src.outputAudioMixerGroup = sfxGroup; // 👈 Route SFX pool
        sfxSources.Add(src);
      }

      // Ambient
      ambientSource = gameObject.AddComponent<AudioSource>();
      ambientSource.loop = true;
      ambientSource.outputAudioMixerGroup = ambientGroup;

      // Stirring, Bubbling, Fire (cauldron)
      stirringSource = gameObject.AddComponent<AudioSource>();
      stirringSource.loop = true;
      stirringSource.outputAudioMixerGroup = sfxGroup;

      bubblingSource = gameObject.AddComponent<AudioSource>();
      bubblingSource.loop = true;
      bubblingSource.outputAudioMixerGroup = sfxGroup;

      fireAmbientSource = gameObject.AddComponent<AudioSource>();
      fireAmbientSource.loop = true;
      fireAmbientSource.outputAudioMixerGroup = ambientGroup;

      // Footsteps
      footstepsSource = gameObject.AddComponent<AudioSource>();
      footstepsSource.loop = false;
      footstepsSource.playOnAwake = false;
      footstepsSource.outputAudioMixerGroup = footstepsGroup;
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

      src.Stop(); // prevent leftover state
      src.clip = clip;
      src.volume = Mathf.Clamp01(volume);
      src.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
      src.Play();
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
      float randomPitch = Random.Range(0.95f, 1.35f);
      PlaySFX(addingOneIngredient, 0.5f, randomPitch);
    }

    public void AddWater()
    {
      if (addingWater == null) return;

      PlaySFX(addingWater, 0.3f, 0.8f);
    }
    #endregion

    #region Footsteps
    // -------------------------------------------------------
    // FOOTSTEPS
    public void PlayFootsteps(AudioClip clip, float speed = 1.6f)
    {
      if (clip == null) return;

      if (!footstepsSource.isPlaying)
      {
        footstepsSource.clip = clip;
        footstepsSource.loop = true;
        footstepsSource.volume = 0.1f;
        footstepsSource.pitch = Random.Range(speed - 0.1f, speed + 0.1f);
        footstepsSource.Play();
      }
    }

    public void StopFootsteps()
    {
      footstepsSource.Stop();
      footstepsSource.clip = null;
    }
    #endregion


    #region Weather
    // -----------------------------------------------------------------------
    // Weather Ambient Methods
    public void CrossfadeAmbient(AudioClip newClip, float fadeDuration = 2f)
    {
        if (newClip == null)
        {
            Debug.LogWarning("[Audio_Manager] Tried to crossfade to a null ambient clip.");
            return;
        }

        // Use the Music_Persistence system directly
        if (Music_Persistence.instance != null)
            Music_Persistence.instance.CheckAmbient(newClip, fadeDuration);
        else
            Debug.LogWarning("[Audio_Manager] No Music_Persistence instance found for CrossfadeAmbient.");
    }

    private IEnumerator CrossfadeAmbientCoroutine(AudioClip newClip, float fadeDuration)
    {
        if (ambientSource == null)
        {
            Debug.LogWarning("[Audio_Manager] Ambient source not initialized.");
            yield break;
        }

        float startVol = ambientSource.volume;

        // If currently playing, fade out
        if (ambientSource.isPlaying && ambientSource.clip != newClip)
        {
            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                ambientSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
                yield return null;
            }
            ambientSource.Stop();
        }

        // Assign new clip and fade in
        ambientSource.clip = newClip;
        ambientSource.volume = 0f;
        ambientSource.loop = true;
        ambientSource.Play();

        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            ambientSource.volume = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
    }
    #endregion


    #region Unique SFX Methods
    public void PlayDoorbell()
    {
      float randomPitch = Random.Range(0.9f, 1.05f);
      PlaySFX(doorBell, 0.32f, randomPitch);
    }

    public void PlaySparkleSFX()
    {
      float[] allowedPitches = { 1.5f, 1.7f };
      float chosenPitch = allowedPitches[Random.Range(0, allowedPitches.Length)];

      PlaySFX(sparkle, 0.3f, chosenPitch);
    }
  }
  #endregion
}

