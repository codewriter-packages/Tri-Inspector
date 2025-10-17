using UnityEngine;
using TriInspector;

public class Decorators_DynamicRangeSample : ScriptableObject
{
    [DynamicRange(nameof(_min), nameof(_max))]
    public int dynamicIntRange = -5;

    [DynamicRange(0, nameof(GetMax))]
    public float dynamicMaxFloatRange = 4.6f;

    [DynamicRange(nameof(minMax))]
    public float dynamicFloatRange = 1.05f;

    public Vector2 minMax = new(-10, 10);
    private int _min = -20;
    private int _max = 20;
    public float GetMax() => 10;
}
