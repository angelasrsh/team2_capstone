using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Canvas : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Quest_Info_SO questInfoForCanvas;

    private string questID;
    private Quest_State currentQuestState;

    private void Awake() {
        questID = questInfoForCanvas.id;
    }
    void OnEnable()
    {
        Game_Events_Manager.Instance.onQuestStateChange += questStateChange;
        
    }

    private void questStateChange(Quest q)
    {
        if (q.Info.id.Equals(questID))
            currentQuestState = q.state;
        
    }
}
