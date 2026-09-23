using TriInspector;
using UnityEngine;

/// <summary>
/// Runs a custom validation method on a field and displays the result as an error or warning in the inspector.
/// </summary>
public class Validators_ValidateInputSample : ScriptableObject
{
    [ValidateInput(nameof(ValidateTexture))]
    public Texture tex;

    [ValidateInput(nameof(ValidateNumber))]
    public int number;

    private TriValidationResult ValidateTexture()
    {
        if (tex == null) return TriValidationResult.Error("Tex is null");
        if (!tex.isReadable) return TriValidationResult.Warning("Tex must be readable");
        return TriValidationResult.Valid;
    }

    private TriValidationResult ValidateNumber()
    {
        if (number == 1)
        {
            return TriValidationResult.Valid;
        }

        return TriValidationResult.Error("Number must be equal 1")
            .WithFix(() => number = 1, "Set to 1");
    }
}