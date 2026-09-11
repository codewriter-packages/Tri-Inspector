using TriInspector;
using UnityEngine;

/// <summary>
/// Groups properties into named tabs, showing only one tab's content at a time in the inspector.
/// </summary>
[DeclareTabGroup("tabs")]
public class Groups_TabGroupSample : ScriptableObject
{
    [Group("tabs"), Tab("One")] public int a;
    [Group("tabs"), Tab("One")] public float b;
    [Group("tabs"), Tab("Two")] public bool c;
    [Group("tabs"), Tab("Two")] public Bounds d;
    [Group("tabs"), Tab("Three")] public Vector3 e;
    [Group("tabs"), Tab("Three")] public Rect f;
}