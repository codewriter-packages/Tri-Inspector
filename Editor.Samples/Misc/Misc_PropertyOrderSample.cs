using TriInspector;
using UnityEngine;

/// <summary>
/// Changes property order in the inspector.
/// </summary>
public class Misc_PropertyOrderSample : ScriptableObject
{
    public float first;

    [PropertyOrder(0)]
    public float second;
}