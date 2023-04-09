using System;
using TriInspector;
using UnityEngine;

[DeclareToggleGroup("My Toggle")]
[DeclareToggleGroup("My Non Collapsible Toggle", Collapsible = false)]
[DeclareToggleGroup("boxed_toggle_struct", Title = "Toggle Struct")]
public class Groups_ToggleGroupSample : ScriptableObject
{
    [Group("My Toggle")] public bool aEnabled = true;
    [Group("My Toggle")] public string b;
    [Group("My Toggle")] public bool c;

    [Group("My Non Collapsible Toggle")] public bool dEnabled;
    [Group("My Non Collapsible Toggle")] public bool e;
    [Group("My Non Collapsible Toggle")] public Vector3 f;
    
    [Group("boxed_toggle_struct"), InlineProperty, HideLabel]
    public MyStruct boxedStruct;

    public MyStruct defaultStruct;

    [Serializable]
    public struct MyStruct
    {
        public bool enabled;
        public int a;
        public float b;
    }
}