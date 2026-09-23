using TriInspector;
using UnityEngine;

/// <summary>
/// Shows non-serialized property in the inspector.
/// </summary>
public class Misc_ShowInInspectorSample : ScriptableObject
{
    private float _field;

    [ShowInInspector]
    private bool _myToggle;

    [ShowInInspector]
    public float ReadOnlyProperty => _field;

    [ShowInInspector]
    public float EditableProperty
    {
        get => _field;
        set => _field = value;
    }
}