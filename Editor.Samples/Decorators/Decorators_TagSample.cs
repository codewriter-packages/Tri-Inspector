using TriInspector;
using UnityEngine;

/// <summary>
/// Renders an string field as a tag selection dropdown in the inspector.
/// </summary>
public class Decorators_TagSample : ScriptableObject
{
    [Tag]
    public string tag;
}