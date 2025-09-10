using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
  public AudioSource audioSource;

  // Plays the audio once
  public void PlayOnce()
  {
    audioSource.PlayOneShot(audioSource.clip);
  }

  // Plays the audio on loop
  public void LoopPlay()
  {
    audioSource.loop = true;
    audioSource.Play();
  }

  // Plays the audio
  public void Play()
  {
    audioSource.Play();
  }

  // Stops the audio
  public void Stop()
  {
    audioSource.Stop();
  }
}
