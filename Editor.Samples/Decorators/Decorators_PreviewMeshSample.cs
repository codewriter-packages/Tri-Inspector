using TriInspector;
using UnityEngine;

public class Decorators_PreviewMeshSample : ScriptableObject
{
    [Title("Shortcut")]
    [InfoBox("Mouse click -> Rotate Mesh in preview")]
    [InfoBox("Shift + Scroll wheel -> Zoom Mesh in preview")]

    [LabelWidth(200f)]
    [PreviewMesh]
    public GameObject mesh;

    [LabelWidth(200f)]
    [PreviewMesh(300)]
    public GameObject meshHeight;

    [LabelWidth(200f)]
    [PreviewMesh(200, 160)]
    public GameObject myObjectLengthAndWidth;

    [LabelWidth(200f)]
    [PreviewMesh(200, 160, false)]
    public GameObject meshNoFoldout;

    [LabelWidth(200f)]
    [PreviewMesh(200, 160, true, PreviewMeshRotationMethod.Freeform)]
    public GameObject meshFreeformRotation;
}
