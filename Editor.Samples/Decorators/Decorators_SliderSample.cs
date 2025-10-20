using UnityEngine;
using TriInspector;

public class Decorators_SliderSample : ScriptableObject
{
    [Slider(nameof(_min), nameof(_max))]
    public int dynamicIntSlider = -6;

    [Slider(0, nameof(GetMax))]
    public float dynamicMaxFloatSlider = 4.6f;

    public Vector2 minMax = new(-10, 10);

    [Slider(nameof(minMax))]
    public float dynamicFloatSlider = 1.83f;

    [Slider(nameof(minMax), autoClamp: true)]
    public int dynamicIntSliderClamped = 4;

    private int _min = -20;
    private int _max = 20;
    public float GetMax() => 10;
}
