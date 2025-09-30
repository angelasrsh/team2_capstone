using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPCs/NPC Data", fileName = "NewNPC")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    public NPCs npcID; // Unique enum identifier
    public string customerName;
    public Sprite overworldSprite;
    public bool datable = true;
    public string lore;

    [Header("Dialog Data")]
    public Character_Portrait_Data portraitData;

    [Header("Preferences")]
    public Dish_Data[] favoriteDishes;
    public Dish_Data[] neutralDishes;
    public Dish_Data[] dislikedDishes;

    // Enum to uniquely identify NPCs
    public enum NPCs
    {
        None,
        Phrog,
        Asper_Agis, 
        // Add all NPCs here
    }

    // [System.Serializable]
    // public class EmotionPortrait
    // {
    //     public enum Emotion
    //     {
    //         Happy,
    //         Sad,
    //         Angry,
    //         Disgusted,
    //         Surprised,
    //         Neutral,
    //         Confused,
    //         Excited,
    //         Worried
    //     }
    //     public Emotion emotion;
    //     public Sprite portrait;
    // }

    // public List<EmotionPortrait> emotionPortraitList = new List<EmotionPortrait>();

    // private Dictionary<EmotionPortrait.Emotion, Sprite> emotionPortraits;

    // public Dictionary<EmotionPortrait.Emotion, Sprite> GetEmotionPortraits()
    // {
    //     if (emotionPortraits == null)
    //     {
    //         emotionPortraits = new Dictionary<EmotionPortrait.Emotion, Sprite>();
    //         foreach (var emotionPortrait in emotionPortraitList)
    //         {
    //             if (!emotionPortraits.ContainsKey(emotionPortrait.emotion))
    //             {
    //                 emotionPortraits.Add(emotionPortrait.emotion, emotionPortrait.portrait);
    //             }
    //         }
    //     }
    //     return emotionPortraits;
    // }
}
