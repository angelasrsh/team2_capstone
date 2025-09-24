using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Controller : MonoBehaviour
{
  public class AnimationController : MonoBehaviour
  {
    private Animator animator;

    void Start()
    {
      animator = GetComponent<Animator>();
    }

    public void PlayAnim(string animName)
    {
      animator.Play(animName);
    }

    public void PauseAnim()
    {
      animator.speed = 0;
    }

    public void ContinueAnim()
    {
      animator.speed = 1;
    }

    public void StopAnim(string idleAnim = "Idle")
    {
      animator.Play(idleAnim, -1, 0f); // Reset to beginning of "Idle"
    }

    public void LoopAnim(string animName)
    {
      animator.Play(animName);
      animator.SetBool("Loop", true); // Needs a loop parameter setup in Animator
    }
  }
}
