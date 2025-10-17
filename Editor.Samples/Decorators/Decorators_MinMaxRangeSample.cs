using UnityEngine;
using TriInspector;

public class Decorators_MinMaxRangeSample : ScriptableObject
{
    [MinMaxRange(0f, 10f)]
    public Vector2 fixedMinMaxRange = new(2f, 4f);

    [MinMaxRange(nameof(_min), nameof(_max))]
    public Vector2Int dynamicIntMinMaxRange = new(-8, 0);

    [MinMaxRange(-20, nameof(GetMax))]
    public Vector2 dynamicFloatMaxRange = new(-7.7f, -1.7f);

    [MinMaxRange(nameof(minMax))]
    public Vector2Int dynamicFloatMinMaxRange = new(0, 4);


    public Vector2 minMax = new(-10, 10);
    private int _min = -20;
    private int _max = 20;
    public float GetMax() => 10;
}
