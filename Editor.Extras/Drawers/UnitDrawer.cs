using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(UnitDrawer), TriDrawerOrder.Decorator)]

namespace TriInspector.Drawers
{
    public class UnitDrawer : TriAttributeDrawer<UnitAttribute>
    {
        private ValueResolver<string> _unitResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _unitResolver = ValueResolver.ResolveString(propertyDefinition, Attribute.unitToDisplay);

            if (_unitResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            var container = new VisualElement
            {
                style =
                {
                    position = Position.Relative,
                },
            };
            container.Add(next);

            var unitLabel = new Label
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    right = 0,
                    top = 0,
                    bottom = 0,
                    unityTextAlign = TextAnchor.MiddleRight,
                    color = Color.grey,
                },
            };
            container.Add(unitLabel);

            container.TrackResolvedValue(property, _unitResolver, "", value => unitLabel.text = value);

            return container;
        }
    }
}