using TriInspector;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// MaterialProperty automatically displays valid shader properties from the target Material,
/// including support for specific types (Float, Color, Vector, Texture, etc.).
/// </summary>
public class Decorators_MaterialPropertySample : ScriptableObject
{
    [MaterialProperty(nameof(material))]
    public string propertyName;

    [MaterialProperty(nameof(material), ShaderPropertyType.Color)]
    public int propertyHash;

    public Material material;
}
