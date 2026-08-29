using TriInspector;
using TriInspector.Drawers;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(SceneDrawer), TriDrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace TriInspector.Drawers
{
    public class SceneDrawer : TriAttributeDrawer<SceneAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            var type = propertyDefinition.FieldType;
            if (type != typeof(string))
            {
                return "Scene attribute can only be used on field with string type";
            }

            return base.Initialize(propertyDefinition);
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var field = new ObjectField
            {
                objectType = typeof(SceneAsset),
                allowSceneObjects = false,
            };

            field.BindTri(property,
                v => AssetDatabase.LoadAssetAtPath<SceneAsset>(v),
                asset => AssetDatabase.GetAssetPath(asset),
                hideLabel: true);

            return new TriAlignedLabelVisualElement<string>(property, field);
        }
    }
}