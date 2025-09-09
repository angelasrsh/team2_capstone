using UnityEngine;
using UnityEngine.UI;

public class DishItemToggle : MonoBehaviour
{
    public Toggle toggle;
    public Outline outline;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        outline.enabled = toggle.isOn;
    }

    private void OnToggleChanged(bool isOn)
    {
        Debug.Log("toggle changed to: " + isOn);
        outline.enabled = toggle.isOn;
    }
}
