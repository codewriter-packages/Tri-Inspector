using UnityEngine;
using TriInspector;

public class Decorators_SliderSampleOrDynamicRangeSample : ScriptableObject
{
    [Slider(nameof(_min), nameof(_max))]
    public int dynamicIntSlider = -5;

    [Slider(0, nameof(GetMax))]
    public float dynamicMaxFloatSlider = 4.6f;

    [Slider(nameof(minMax))]
    public float dynamicFloatSlider = 1.05f;

    [DynamicRange(nameof(minMax))]
    public float dynamicFloatRange = 1.05f;

    public Vector2 minMax = new(-10, 10);
    private int _min = -20;
    private int _max = 20;
    public float GetMax() => 10;
}
