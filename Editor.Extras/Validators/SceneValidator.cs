using TriInspector;
using TriInspector.Validators;
using UnityEditor;

[assembly: RegisterTriAttributeValidator(typeof(SceneValidator), ApplyOnArrayElement = true)]

namespace TriInspector.Validators
{
    public class SceneValidator : TriAttributeValidator<SceneAttribute>
    {
        public override TriValidationResult Validate(TriProperty property)
        {
            if (property.FieldType == typeof(string))
            {
                var value = (string) property.Value;

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(value) == null)
                {
                    return TriValidationResult.Error($"{value} not a valid scene");
                }

                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (!property.Comparer.Equals(value, scene.path))
                    {
                        continue;
                    }

                    if (!scene.enabled)
                    {
                        return TriValidationResult.Error($"{value} disabled in build settings");
                    }

                    return TriValidationResult.Valid;
                }

                return TriValidationResult.Error($"{value} not added to build settings");
            }

            return TriValidationResult.Valid;
        }
    }
}