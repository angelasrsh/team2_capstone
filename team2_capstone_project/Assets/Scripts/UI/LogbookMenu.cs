using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogbookMenu : MonoBehaviour
{
  private bool otherDishDetailsOpen = false;
  public GameObject Logbook;
  private bool isPaused = false; // Currently will overlap pause menu, I think

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.J))
    {
      if (isPaused)
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

  // Display dish details
  public void ShowDishDetails(string dishName)
  {
    if (otherDishDetailsOpen)
    {
      Debug.Log("Another dish details panel is open. Replacing with new dish details.");
      // Replace old dish details with new one
      //
    }
    else
    {
      otherDishDetailsOpen = true;
      Debug.Log("Showing details for dish: " + dishName);
      // Display dish details
      //
    }
  }
}
