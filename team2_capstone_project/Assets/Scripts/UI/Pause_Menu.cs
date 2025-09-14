using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Menu : MonoBehaviour
{
  public GameObject menuBox;
  public GameObject darkOverlay;
  private bool isPaused = false;

  // Update is called once per frame
  void Update()
  {
      if(Input.GetKeyDown(KeyCode.Escape))
      {
          if(isPaused)
            ResumeGame();
          else
            PauseGame();
      }
  }

  public void PauseGame()
  {
    Debug.Log("Pausing game...");
    menuBox.SetActive(true);
    darkOverlay.SetActive(true);
    isPaused = true;
  }

  // Resume the game from the pause menu
  public void ResumeGame()
  {
    Debug.Log("Resuming game...");
    menuBox.SetActive(false);
    darkOverlay.SetActive(false);
    isPaused = false;
  }

  // Quit the game from pause menu and return to the main menu
  public void QuitGame()
  {
    Debug.Log("Quitting game...");
    SceneManager.LoadScene("Main_Menu");
  }
}
