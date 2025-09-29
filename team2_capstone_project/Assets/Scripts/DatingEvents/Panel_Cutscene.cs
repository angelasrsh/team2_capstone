using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Panel_Cutscene : MonoBehaviour
{
    [Header("Scene panels")]
    public Event_Data DatingCutsceneData;

    // For scene-fading- must be gameobjects with ImageComponents on them
    //private GameObject[] panels = new GameObject[2];
    UnityEngine.UI.Image[] panelObjects;

    private int panelIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        panelObjects = GetComponentsInChildren<UnityEngine.UI.Image>();

        if (panelObjects.Length != 2)
            Debug.LogWarning($"[P_CUT] panels array contains {panelObjects.Length} panel gameObjects with image components instead of 2");

    }

    // Show/hide panels to advance slideshow
    public void ChangePanel()
    {
        // If not the first panel, hide the old panel
        if (panelIndex > 0)
            hidePanel(panelIndex - 1);

        // If not the last panel, display a new panel
        if (panelIndex < DatingCutsceneData.panels.Length)
            displayPanel(panelIndex);       

        panelIndex++;

    }

    /// <summary>
    /// Fade in a child panel object displaying the given index
    /// </summary>
    /// <param name="index"> Index of the DatingCutsceneData panel to display </param>
    public void displayPanel(int index)
    {
        int panelObjIndex = index % 2; // Alternate between the two panels
        if (index < DatingCutsceneData.panels.Length)
        {
            // Change image
            panelObjects[panelObjIndex].sprite = DatingCutsceneData.panels[index];

            // Set opacity of image to 1
            UnityEngine.UI.Image image = panelObjects[panelObjIndex];
            Color color = image.color;
            // StartCoroutine(FadeSprites(image, 1.0f, 10));
            color.a = 1;
            image.color = color;
        }
    }

    // Make a panel disappear
    public void hidePanel(int index)
    {
        int panelObjIndex = index % 2; // Alternate between the two panels
        // Set opacity of image to 0
        UnityEngine.UI.Image image = panelObjects[panelObjIndex];
        Color color = image.color;
        // StartCoroutine(FadeSprites(image, 1.0f, 10));
        color.a = 0;
        image.color = color;

    }

    public void onClickNext()
    {
        if (panelIndex < DatingCutsceneData.panels.Length)
            ChangePanel();
        else
            SceneManager.LoadScene("Restaurant");
    }

    // IEnumerator FadeSprites(UnityEngine.UI.Image sprite, float newAlpha, float fadeDuration)
    // {
    //     float startAlpha = sprite.color.a;
    //     float time = 0;

    //     while (time < fadeDuration)
    //     {
    //         time += Time.deltaTime;
    //         float currentAlpha = Mathf.Lerp(startAlpha, newAlpha, time / fadeDuration);
    //         Color color = sprite.color;
    //         color.a = currentAlpha;
    //         sprite.color = color;
    //         yield return null;
    //     }

    //     sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, newAlpha);
    // }
}
