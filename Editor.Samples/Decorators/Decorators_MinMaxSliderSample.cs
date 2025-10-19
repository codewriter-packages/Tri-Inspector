using UnityEngine;
using TriInspector;

public class Decorators_MinMaxSliderSample : ScriptableObject
{
    [MinMaxSlider(0f, 10f)]
    public Vector2 fixedMinMaxSlider = new(2f, 4f);

    [MinMaxSlider(nameof(_min), nameof(_max))]
    public Vector2Int dynamicIntMinMaxSlider = new(-8, 0);

    [MinMaxSlider(-20, nameof(GetMax))]
    public Vector2 dynamicFloatMaxSlider = new(-7.7f, -1.7f);

    public Vector2 minMax = new(-10, 10);

    [MinMaxSlider(nameof(minMax))]
    public Vector2 dynamicFloatMinMaxSlider = new(0, 4);

    [MinMaxSlider(nameof(minMax), autoClamp: true)]
    public Vector2Int dynamicIntMinMaxSliderClamped = new(2, 6);


    private int _min = -20;
    private int _max = 20;
    public float GetMax() => 10;
}
