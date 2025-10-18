using TriInspector;
using UnityEngine;

public class Decorators_AssetDropdownSample : ScriptableObject
{
    // The full syntax of filters can be found here:
    // https://docs.unity3d.com/Documentation/ScriptReference/AssetDatabase.FindAssets.html

    // Dropdown list of textures located in the `Assets/Textures/Character` folder
    [AssetDropdown("t: Texture2D", searchInFolders: new[] {"Assets/Textures/Character"})]
    public Texture2D character;

    // Custom dropdown
    [Required, BuildingDropdown]
    public BuildingConfig myBuilding;
}

// Custom dropdown attribute of assets with `BuildingConfig` type
public class BuildingDropdownAttribute : AssetDropdownAttribute
{
    public BuildingDropdownAttribute() : base($"t: {nameof(BuildingConfig)}") { }

    // Overrides default names of dropdown elements
    public override string GetDisplayName(Object asset) => asset is BuildingConfig buildingConfig
        ? buildingConfig.niceName
        : base.GetDisplayName(asset);
}

// Demo example configuration file
public class BuildingConfig : ScriptableObject
{
    public string niceName;
}