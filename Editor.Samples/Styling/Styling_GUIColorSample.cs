using TriInspector;
using UnityEngine;

public class Styling_GUIColorSample : ScriptableObject
{
    [GUIColor(0.8f, 1.0f, 0.6f)]
    public Vector3 vec;

    [GUIColor("0000FF")]
    [Button]
    public void BlueButton()
    {
    }

    [GUIColor("cyan")]
    [Button]
    public void CyanButton()
    {
    }

    [GUIColor("$GetGreenColor")]
    [Button]
    public void GreenButton()
    {
    }

    [GUIColor(255, 75, 75)]
    [Button]
    public void RedButton()
    {
    }

    [GUIColor("$GetColor")]
    [Button(ButtonSizes.Large)]
    public void ColoredButton()
    {
    }
    
    private Color GetGreenColor => Color.green;
    
    private Color GetColor
    {
        get
        {
            var time = (float) UnityEditor.EditorApplication.timeSinceStartup;
            var hue = time * 0.225f % 1f;
            var color = Color.HSVToRGB(hue, 1f, 1f);
            return color;
        }
    }
}