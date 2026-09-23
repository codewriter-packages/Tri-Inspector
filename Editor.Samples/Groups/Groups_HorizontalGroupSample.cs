using TriInspector;
using UnityEngine;

/// <summary>
/// Lays out properties side by side in a horizontal row in the inspector.
/// </summary>
[DeclareHorizontalGroup("vars")]
[DeclareHorizontalGroup("buttons")]
public class Groups_HorizontalGroupSample : ScriptableObject
{
    [Group("vars")] public int a;
    [Group("vars")] public int b;
    [Group("vars")] public int c;

    [Button, Group("buttons")]
    public void ButtonA()
    {
    }

    [Button, Group("buttons")]
    public void ButtonB()
    {
    }
}