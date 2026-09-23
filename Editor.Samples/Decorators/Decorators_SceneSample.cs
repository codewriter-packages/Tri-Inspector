using System.Collections.Generic;
using TriInspector;
using UnityEngine;

/// <summary>
/// Renders a string field as a scene picker dropdown populated from the project's build settings.
/// </summary>
public class Decorators_SceneSample : ScriptableObject
{
    [Scene] public string scene;

    [Scene] public List<string> scenes;
}