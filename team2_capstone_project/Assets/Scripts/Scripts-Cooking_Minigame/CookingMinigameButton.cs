using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CookingMinigameButton : MonoBehaviour
{
    private string cafeSceneName = "Restaurant";

    public void BackButtonPressed()
    {
        StartCoroutine(LoadCafe());
    }

    private IEnumerator LoadCafe()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(cafeSceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
