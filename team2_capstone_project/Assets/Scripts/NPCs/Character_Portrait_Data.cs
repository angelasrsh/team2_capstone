using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterPortraits", menuName = "NPCs/Character Portrait Data")]
public class Character_Portrait_Data : ScriptableObject
{
    public enum CharacterName
    {
        Asper_Agis,
        Phrog, 
        Satyr
    }
    public CharacterName characterName;
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
            foreach (var emotionPortrait in emotionPortraitList)
            {
                if (!emotionPortraits.ContainsKey(emotionPortrait.emotion))
                {
                    emotionPortraits.Add(emotionPortrait.emotion, emotionPortrait.portrait);
                }
            }
        }
        return emotionPortraits;
    }
}
