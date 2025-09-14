using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Chop_Board_Setup : MonoBehaviour, ICustomDrag
{
    private RectTransform rectTransform;
    [SerializeField] RectTransform choppingZone; //make private later
    // Start is called before the first frame update
    void Start()
    {
        //clear the chopping board
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCurrentDrag()
    {
        
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }
}
