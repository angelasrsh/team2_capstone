using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Auto_Shadow_UI_Graphic : MonoBehaviour
{
    public Vector2 shadowDistance = new Vector2(2f, -2f);
    public Color shadowColor = new Color(0, 0, 0, 0.5f);

    void Awake()
    {
        Graphic graphic = GetComponent<Graphic>();
        if (graphic == null) return;

        Shadow shadow = graphic.GetComponent<Shadow>();
        if (shadow == null)
            shadow = graphic.gameObject.AddComponent<Shadow>();

        shadow.effectColor = shadowColor;
        shadow.effectDistance = shadowDistance;
    }
}
