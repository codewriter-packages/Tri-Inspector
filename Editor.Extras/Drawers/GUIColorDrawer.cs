using JetBrains.Annotations;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(GUIColorDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
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
                next.style.backgroundColor = value;
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