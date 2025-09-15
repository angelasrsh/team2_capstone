using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Buttons : MonoBehaviour
{
  // Start the game from the main menu
  public void StartGame()
  {
    Debug.Log("Starting game...");
    LoadScene("World_Map"); // Change "Restaurant" to whichever scene goes next!!!!!!!!
  }

  // Load a specific scene by name
  public void LoadScene(string sceneName)
  {
    Debug.Log("Loading scene: " + sceneName);
    SceneManager.LoadScene(sceneName);
  }

  // Open a specific panel (e.g., settings, credits)
  public void OpenPanel(GameObject panel)
  {
    panel.SetActive(true);
  }

  // Close a specific panel
  public void ClosePanel(GameObject panel)
  {
    panel.SetActive(false);
  }

  // Quit the application
  public void QuitGame()
  {
    Debug.Log("Quitting game...");
    Application.Quit();
  }
}
