using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Input_Controller : MonoBehaviour
{
  public static Player_Input_Controller instance;

  void Awake()
  {
    if (instance != null && instance != this)
    {
      Destroy(gameObject);
      return;
    }

    instance = this;
    DontDestroyOnLoad(gameObject);
  }

  public void DisablePlayerInput()
  {
    GetComponent<PlayerInput>().enabled = false;
  }

  public void EnablePlayerInput()
  {
    GetComponent<PlayerInput>().enabled = true;
  }   
}
