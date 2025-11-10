using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene_Manager : MonoBehaviour
{
    public static Cutscene_Manager Instance;
    private HashSet<string> playedCutscenes = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void LoadFromSave(GameData data) => playedCutscenes = new HashSet<string>(data.playedCutscenes);
    public void SaveToGameData(GameData data) => data.playedCutscenes = new List<string>(playedCutscenes);
    public bool HasPlayed(string cutsceneID) => playedCutscenes.Contains(cutsceneID);
    public void MarkAsPlayed(string cutsceneID)
    {
        if (!string.IsNullOrEmpty(cutsceneID))
            playedCutscenes.Add(cutsceneID);
    }
}
