using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit_Minigame_If_Full : MonoBehaviour
{
    private void OnEnable()
    {
       Game_Events_Manager.Instance.onDishInventoryFull += DishInventoryFull; 
    }

    private void OnDisable()
    {
       Game_Events_Manager.Instance.onDishInventoryFull -= DishInventoryFull; 
    }


    private void DishInventoryFull()
    {
        StartCoroutine(leave_scene());
    }

    private IEnumerator leave_scene()
    {
        yield return new WaitForSeconds(2); // Wait for other code to finish(?)
        Room_Change_Trigger change_trigger = FindObjectOfType<Room_Change_Trigger>();

        if (change_trigger != null)
            change_trigger.OnBackButtonPressedForMinigame();
        else
            Debug.Log("[Exit_Minigame_If_Full] Trying to exit minigame but no Room_Change_Trigger was found in the scene.");
        
    }


}
