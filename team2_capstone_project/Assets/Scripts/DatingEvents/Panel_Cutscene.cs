using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Grimoire;

public class Panel_Cutscene : MonoBehaviour
{
    
    // For scene-fading- must be gameobjects with ImageComponents on them
    //private GameObject[] panels = new GameObject[2];
    UnityEngine.UI.Image[] panelObjects;

    // [Header("Set using Affection_System")]
    [SerializeField] private Event_Data DatingCutsceneData;
    private int panelIndex = 0;
    private bool loadingRoom = false;

    private Dialogue_Manager dm;

    // Start is called before the first frame update
    void Start()
    {

        //dm =  UnityEngine.Object.FindObjectOfType<Dialogue_Manager>();
        dm = FindObjectOfType<Dialogue_Manager>();

        // Get cutscene to play
        DatingCutsceneData = Affection_System.Instance.Cutscene;
        if (DatingCutsceneData == null)
        {
            Helpers.printLabeled(this, "Warning: No cutscene has been set in the Affection System on GameManager");
            StartCoroutine(TransitionBackToRestaurant());
        }

        panelObjects = GetComponentsInChildren<UnityEngine.UI.Image>();

        if (panelObjects.Length != 2)
            Debug.LogWarning($"[P_CUT] panels array contains {panelObjects.Length} panel gameObjects with image components instead of 2");

        ChangePanel();

        Audio_Manager.instance?.PlayMusic(DatingCutsceneData.Music);

    }

    // Show/hide panels to advance slideshow
    public void ChangePanel()
    {
        // If not the first panel, hide the old panel
        if (panelIndex > 0)
            hidePanel(panelIndex - 1);

        // If not the last panel, display a new panel
        if (panelIndex < DatingCutsceneData.Panels.Length)
            displayPanel(panelIndex);

        if (DatingCutsceneData.Panels[panelIndex].DialogKeys.Count > 0)
            dm.PlaySceneMultiple(DatingCutsceneData.Panels[panelIndex].DialogKeys);

        panelIndex++;
    }

    /// <summary>
    /// Fade in a child panel object displaying the given index
    /// </summary>
    /// <param name="index"> Index of the DatingCutsceneData panel to display </param>
    public void displayPanel(int index)
    {
        int panelObjIndex = index % 2; // Alternate between the two panels
        if (index < DatingCutsceneData.Panels.Length)
        {
            // Change image
            panelObjects[panelObjIndex].sprite = DatingCutsceneData.Panels[index].Panel;

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
        if (panelIndex < DatingCutsceneData.Panels.Length)
        {
            ChangePanel();
        }
        else
        {
            if (DatingCutsceneData.Customer == null) // global event like intro cutscene
                Player_Progress.Instance.SetIntroPlayed(true);
            else if (Cutscene_Manager.Instance != null) // Mark cutscene as played
                Cutscene_Manager.Instance.MarkAsPlayed(DatingCutsceneData.CutsceneID);

            // Save immediately to persist this
            Save_Manager.instance?.AutoSave();

            if (!loadingRoom)
            {
                Room_Change_Manager.instance.GoToRoom(Room_Data.RoomID.Dating_Events, DatingCutsceneData.roomToReturnTo);
                loadingRoom = true;
            }
        }
    }

    private IEnumerator TransitionBackToRestaurant()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Updated_Restaurant", LoadSceneMode.Single);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
            yield return null;

        Debug.Log("[Panel_Cutscene] Returned to restaurant from cutscene.");
    }


    // TODO: Would like to try fading the cutscene panels for a smoother transition
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
