using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogbookMenu : MonoBehaviour
{
  public GameObject Logbook;
  private bool isPaused = false; // Currently will overlap pause menu, I think

  // Update is called once per frame
  void Update()
  {
      if(Input.GetKeyDown(KeyCode.J))
      {
          if(isPaused)
            ResumeGame();
          else
            PauseGame();
      }
  }

  public void PauseGame()
  {
    Debug.Log("Opening Logbook and pausing game...");
    Logbook.SetActive(true);
    isPaused = true;
  }

  // Resume the game from the pause menu
  public void ResumeGame()
  {
    Debug.Log("Closing Logbook and resuming game...");
    Logbook.SetActive(false);
    isPaused = false;
  }

}
