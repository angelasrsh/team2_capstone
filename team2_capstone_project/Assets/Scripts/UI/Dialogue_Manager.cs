using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogue_Manager : MonoBehaviour
{

    [SerializeField]
    private Image character;
    [SerializeField]
    private TextMeshProUGUI dialogue;
    [SerializeField]
    private Canvas c;

    public Queue<string> dialogue_queue = new Queue<string>();
    public Queue<AudioClip> clipq = new Queue<AudioClip>();

    private double yapstart;
    private int yapspeed = 20;
    private string to_yap = "";

    // Start is called before the first frame update
    void Start()
    {
        c.gameObject.SetActive(false);
    }

    public void ChangeYapSpeed(int ys) {
        yapspeed = ys;
    }

    public void Changecharacter(Sprite i) {
        character.overrideSprite = i;
    }

    public bool Done() {
        return to_yap == "" && dialogue_queue.Count == 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (to_yap.Length > 0) {
            var charsToShow = (int)((Time.timeAsDouble - this.yapstart) * (double)yapspeed);
            charsToShow = Mathf.Min(charsToShow, to_yap.Length);
            this.dialogue.text = this.to_yap.Substring(0, charsToShow);
        } else if(dialogue_queue.Count > 0) {
            StartCoroutine(ShowOneDialogue(dialogue_queue.Dequeue()));
            FindObjectOfType<Dialgoue_SFX_Manager>().Play(clipq.Dequeue());
        } else {
            c.gameObject.SetActive(false);
        }
    }

    public void QueueDialogue(string t) {
        dialogue_queue.Enqueue(t);
        clipq.Enqueue(null);
    }

    public void QueueDialogueWithSound(string t, AudioClip c) {
        dialogue_queue.Enqueue(t);
        clipq.Enqueue(c);
    }

    public IEnumerator ShowOneDialogue(string text) {
        c.gameObject.SetActive(true);

        this.yapstart = Time.timeAsDouble;
        this.to_yap = text;
        int charsShown;
        yield return new WaitWhile(() => {
            charsShown = (int)((Time.timeAsDouble - this.yapstart) * (double)yapspeed);
            return charsShown < to_yap.Length;
        });
        if (dialogue_queue.Count > 0) {
            yield return new WaitForSeconds(2f);
        }
        else {
            yield return new WaitForSeconds(3.5f);
        }
        this.to_yap = "";
    }

}