using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hide_If_Not_Mobile : MonoBehaviour
{
  private static Hide_If_Not_Mobile Instance;
  void Awake()
  {
    // Disable this UI if the device isn't a handheld/mobile device
    bool simulateMobile = false;

    // #if UNITY_EDITOR
    //   simulateMobile = true; // comment this back in with the #if and #endif if you want to simulate mobile in editor
    // #endif
    
    if (simulateMobile == false && !(Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld))
    {
      Destroy(this.gameObject);
      return;
    }

    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
  }
}
