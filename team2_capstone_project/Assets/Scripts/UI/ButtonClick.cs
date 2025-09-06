using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{
  // Set this in the Inspector
  public string sceneToLoad;

    // Start is called before the first frame update
  void Start()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClick);
    }

    // Called once per frame
    void OnClick()
    {
        Debug.Log("Button Clicked: Changing scene to: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
