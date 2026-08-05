using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;
using UnityEngine.UIElements;

[assembly: RegisterTriAttributeDrawer(typeof(OnValueChangedDrawer), TriDrawerOrder.System)]

namespace TriInspector.Drawers
{
    public class OnValueChangedDrawer : TriAttributeDrawer<OnValueChangedAttribute>
    {
        private ActionResolver _actionResolver;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _actionResolver = ActionResolver.Resolve(propertyDefinition, Attribute.Method);
            if (_actionResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return TriExtensionInitializationResult.Ok;
        }

        public override VisualElement CreateVisualElement(TriProperty property, VisualElement next)
        {
            void OnValueChanged(TriProperty _)
            {
                property.PropertyTree.ApplyChanges();
                _actionResolver.InvokeForAllTargets(property);
                property.PropertyTree.Update();
            }

            next.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                property.ValueChanged += OnValueChanged;
                property.ChildValueChanged += OnValueChanged;
            });

            next.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                property.ChildValueChanged -= OnValueChanged;
                property.ValueChanged -= OnValueChanged;
            });

            return next;
        }
    }
}
