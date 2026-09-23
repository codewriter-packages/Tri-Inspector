using TriInspector;
using UnityEngine;

/// <summary>
/// Renders a property as a read-only text label in the inspector instead of an editable field.
/// </summary>
public class Decorators_DisplayAsStringSample : ScriptableObject
{
    [DisplayAsString]
    public string hello = "world";

    [DisplayAsString, HideLabel]
    public Texture2D texture;
}