using JetBrains.Annotations;
using TriInspector.Resolvers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Drawers
{
    [RegisterTriAttributeDrawer(TriDrawerOrder.Decorator)]
    public class GUIColorDrawer : TriAttributeDrawer<GUIColorAttribute>
    {
        [CanBeNull] private ValueResolver<Color> _colorResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!string.IsNullOrEmpty(Attribute.GetColor))
            {
                _colorResolver = ValueResolver.Resolve<Color>(propertyDefinition, Attribute.GetColor);
            }

            if (_colorResolver != null && _colorResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            void SetColor(Color value)
            {
                Color.RGBToHSV(value, out var h, out var s, out var v);

                var buttonColor = EditorGUIUtility.isProSkin
                    ? Color.HSVToRGB(h, s * 0.7f, v * 0.4f)
                    : Color.HSVToRGB(h, s * 1.0f, v * 0.9f);

                var inputFieldColor = EditorGUIUtility.isProSkin
                    ? Color.HSVToRGB(h, s * 0.7f, v * 0.165f)
                    : Color.HSVToRGB(h, s * 1.0f, v * 0.9f);

                var textColor = EditorGUIUtility.isProSkin
                    ? Color.HSVToRGB(h, s * 1.0f, v * 1.0f)
                    : Color.HSVToRGB(h, s * 1.0f, v * 0.2f);

                next.Query<Label>()
                    .ForEach(label => label.style.color = textColor);
                next.Query(className: "unity-base-field__input")
                    .ForEach(input => input.style.color = textColor);
                next.Query(className: "unity-base-text-field__input")
                    .ForEach(input => input.style.backgroundColor = inputFieldColor);
                next.Query(className: "unity-toggle__checkmark")
                    .ForEach(checkmark => checkmark.style.unityBackgroundImageTintColor = value);
                next.Query<Button>().ForEach(button =>
                {
                    button.style.color = textColor;
                    button.style.backgroundColor = buttonColor;
                });
            }

            if (_colorResolver != null)
            {
                next.TrackResolvedValue(property, _colorResolver, Attribute.Color, SetColor);
            }
            else
            {
                SetColor(Attribute.Color);
            }

            return next;
        }
    }
}