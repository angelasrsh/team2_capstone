using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// An instance of this exists in the data folder to hold all the Quest_Info_SOs.
/// Used by the Quest Manager.
/// </summary>
[CreateAssetMenu(fileName = "Quest_Database", menuName = "Quest/Quest_Database")]
public class Quest_Database : ScriptableObject
{
    public List<Quest_Info_SO> allQuests;
    public bool ToggleToRefresh;

    public void PopulateQuests()
    {
        allQuests.Clear();

        string t = "t:" + typeof(Quest_Info_SO).Name;
        string[] p = new[] { AssetDatabase.GetAssetPath(this) };

        string[] guids = AssetDatabase.FindAssets("t:" + typeof(Quest_Info_SO).Name, new[] {"Assets/Data/Quests"});
        //string[] guids = AssetDatabase.FindAssets("t:" + typeof(Quest_Info_SO).Name, "new[] { AssetDatabase.GetAssetPath(this) }");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Quest_Info_SO quest = AssetDatabase.LoadAssetAtPath<Quest_Info_SO>(assetPath);
            if (quest != null)
                allQuests.Add(quest);
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        PopulateQuests();
#endif
    }
}
