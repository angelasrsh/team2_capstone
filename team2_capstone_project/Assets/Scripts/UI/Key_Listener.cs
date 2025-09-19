using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Key_Listener : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (SceneManager.GetActiveScene().name == "World_Map")
            {
                Choose_Menu_Items menu = FindObjectOfType<Choose_Menu_Items>();
                if (menu != null && menu.HasSelectedDishes())
                {
                    Debug.Log("Finished resource gathering. Loading Restaurant scene...");
                    SceneManager.LoadScene("Restaurant");
                }
                else
                {
                    Debug.Log("Please select dishes before continuing.");
                    // maybe trigger the error message display here as well
                }
            }
        }
    }


}
