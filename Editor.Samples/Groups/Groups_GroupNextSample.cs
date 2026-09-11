using TriInspector;
using UnityEngine;

/// <summary>
/// Groups consecutive properties into a group without repeating the [Group] attribute on each field.
/// Use [UnGroupNext] to stop grouping.
/// </summary>
[DeclareBoxGroup("box", Title = "Box")]
[DeclareHorizontalGroup("box/split")]
[DeclareBoxGroup("box/split/left", Title = "Left")]
[DeclareBoxGroup("box/split/right", Title = "Right")]
public class Groups_GroupNextSample : ScriptableObject
{
    public float header;

    [GroupNext("box")]
    public float boxA;

    public float boxB;

    [GroupNext("box/split/left")]
    public float boxLeftA;

    public float boxLeftB;

    [GroupNext("box/split/right")]
    public float boxRightA;

    public float boxRightB;

    [UnGroupNext]
    public float footerA;

    public float footerB;
}
