using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NPCs/NPC Data", fileName = "NewNPC")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    public NPCs npcID; // Internal enum key
    public string customerName; // Display name (e.g. "Asper Aigis" that can include spaces)
    public Sprite overworldSprite;
    public RuntimeAnimatorController walkAnimatorController;
    public bool datable = true;
    public string lore;

    [Header("Dialog Audio Settings")]
    public AudioClip dialogClipSound;
    public float pitchMin = 0.95f; 
    public float pitchMax = 1.05f; 
    public float textSpeed = 0.025f; 

    [Header("Portrait Data")]
    public Sprite defaultPortrait;

    [System.Serializable]
    public class EmotionPortrait
    {
        public enum Emotion
        {
            Happy,
            Sad,
            Angry,
            Disgusted,
            Surprised,
            Neutral,
            Confused,
            Excited,
            Worried
        }
        public Emotion emotion;
        public Sprite portrait;
    }

    public List<EmotionPortrait> emotionPortraitList = new List<EmotionPortrait>();

    private Dictionary<EmotionPortrait.Emotion, Sprite> emotionPortraits;
    public Dictionary<EmotionPortrait.Emotion, Sprite> GetEmotionPortraits()
    {
        if (emotionPortraits == null)
        {
            emotionPortraits = new Dictionary<EmotionPortrait.Emotion, Sprite>();
            foreach (var ePortrait in emotionPortraitList)
            {
                if (!emotionPortraits.ContainsKey(ePortrait.emotion))
                {
                    emotionPortraits.Add(ePortrait.emotion, ePortrait.portrait);
                }
            }
        }
        return emotionPortraits;
    }

    [Header("Preferences")]
    public Dish_Data[] favoriteDishes;
    public Dish_Data[] neutralDishes;
    public Dish_Data[] dislikedDishes;

    // Enum to uniquely identify NPCs
    public enum NPCs
    {
        None,
        Phrog,
        Elf,
        Satyr
    }
}
