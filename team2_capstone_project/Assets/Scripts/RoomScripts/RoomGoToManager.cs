using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Grimoire;

public class RoomGoToManager : MonoBehaviour
{
    public static RoomGoToManager instance;
    private ScreenFade blackScreenFade;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // e.g. RoomGoToManager.instance.GoToRoom(RoomData.RoomID.Restaurant, RoomData.RoomID.CookingMinigame);
    public void GoToRoom(RoomData.RoomID currentRoomID, RoomData.RoomID exitingTo)
    {
        RoomData currentRoom = RoomManager.GetRoom(currentRoomID);
        RoomExitOptions exit = Exit(currentRoom, exitingTo);

        if (exit != null && exit.targetRoom != null)
        {
            StartCoroutine(HandleRoomTransition(exit.targetRoom, exit.spawnPointID));
        }
        else
        {
            Debug.LogError($"Exit not found from {currentRoomID} to {exitingTo}");
        }
    }

    private RoomExitOptions Exit(RoomData room, RoomData.RoomID exitingTo)
    {
        foreach (var exit in room.exits)
        {
            if (exit.exitingTo == exitingTo)
                return exit;
        }
        return null;
    }

    private IEnumerator HandleRoomTransition(RoomData targetRoom, RoomData.SpawnPointID spawnPointID)
    {
        blackScreenFade = FindObjectOfType<ScreenFade>();
        if (blackScreenFade == null)
        {
            Debug.LogWarning("blackScreenFade missing.");
            yield break;
        }

        // Fade out music/ambient
        MusicPersistence.instance.PreTransitionCheckMusic(targetRoom.music);
        MusicPersistence.instance.PreTransitionCheckAmbient(targetRoom.ambientSound);

        // Fade screen to black
        blackScreenFade.StartCoroutine(blackScreenFade.BlackFadeIn());
        yield return new WaitForSeconds(blackScreenFade.fadeDuration);

        // Load new room
        yield return StartCoroutine(ChangeRoom(targetRoom, spawnPointID));
    }

    private IEnumerator ChangeRoom(RoomData targetRoom, RoomData.SpawnPointID spawnPointID)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetRoom.roomID.ToString());
        while (!asyncLoad.isDone)
            yield return null;

        // Fade out black
        blackScreenFade = FindObjectOfType<ScreenFade>();
        if (blackScreenFade != null)
        {
            yield return StartCoroutine(HandleNewRoomTransition(targetRoom));
        }

        // Place player at spawn point if overworld scene
        if (targetRoom.isOverworldScene)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
                foreach (var sp in spawnPoints)
                {
                    if (sp.spawnPointID == spawnPointID)
                    {
                        player.transform.position = sp.transform.position;
                        break;
                    }
                }
            }
        }

        // Handle music
        if (targetRoom.music != null)
            MusicPersistence.instance.CheckMusic(targetRoom.music);
        else
            MusicPersistence.instance.StopMusic();

        // Handle ambient
        if (targetRoom.ambientSound != null)
            MusicPersistence.instance.CheckAmbient(targetRoom.ambientSound);
        else
            MusicPersistence.instance.StopAmbient();
    }

    private IEnumerator HandleNewRoomTransition(RoomData targetRoom)
    {
        if (blackScreenFade == null)
            yield break;

        blackScreenFade.fadeCanvasGroup.alpha = 1;

        yield return blackScreenFade.StartCoroutine(blackScreenFade.BlackFadeOut());
    }
}
