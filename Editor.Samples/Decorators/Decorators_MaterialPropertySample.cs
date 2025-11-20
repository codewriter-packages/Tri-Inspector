using TriInspector;
using UnityEngine;
using UnityEngine.Rendering;

public class Decorators_MaterialPropertySample : ScriptableObject
{
    [MaterialProperty(nameof(material))]
    public string propertyName;

    [MaterialProperty(nameof(material), ShaderPropertyType.Color)]
    public int propertyHash;

    public Material material;
}
