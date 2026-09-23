using TriInspector;
using UnityEngine;

/// <summary>
/// Renders an integer field as a layer selection dropdown in the inspector.
/// </summary>
public class Decorators_LayerSample : ScriptableObject
{
    public int defaultLayer;

    [Layer]
    public int layer;
}
