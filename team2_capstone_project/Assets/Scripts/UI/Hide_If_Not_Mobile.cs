using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hide_If_Not_Mobile : MonoBehaviour
{
    void Awake()
    {
        // Disable this UI if the device isn't a handheld/mobile device
        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            gameObject.SetActive(false);
        }
    }
}
