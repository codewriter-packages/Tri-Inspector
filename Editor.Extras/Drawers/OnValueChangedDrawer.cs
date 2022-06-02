using TriInspector;
using TriInspector.Drawers;
using TriInspector.Resolvers;

[assembly: RegisterTriAttributeDrawer(typeof(OnValueChangedDrawer), TriDrawerOrder.System)]

namespace TriInspector.Drawers
{
    public class OnValueChangedDrawer : TriAttributeDrawer<OnValueChangedAttribute>
    {
        private ActionResolver _actionResolver;

        public override void Initialize(TriPropertyDefinition propertyDefinition)
        {
            base.Initialize(propertyDefinition);

            _actionResolver = ActionResolver.Resolve(propertyDefinition, Attribute.Method);
        }

        public override string CanDraw(TriProperty property)
        {
            if (_actionResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return base.CanDraw(property);
        }

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new OnValueChangedListenerElement(property, next, _actionResolver);
        }

        private class OnValueChangedListenerElement : TriElement
        {
            private readonly TriProperty _property;
            private readonly ActionResolver _actionResolver;

            public OnValueChangedListenerElement(TriProperty property, TriElement next, ActionResolver actionResolver)
            {
                _property = property;
                _actionResolver = actionResolver;

                AddChild(next);
            }

            protected override void OnAttachToPanel()
            {
                base.OnAttachToPanel();

                _property.ValueChanged += OnValueChanged;
                _property.ChildValueChanged += OnValueChanged;
            }

            protected override void OnDetachFromPanel()
            {
                _property.ChildValueChanged -= OnValueChanged;
                _property.ValueChanged -= OnValueChanged;

                base.OnDetachFromPanel();
            }

            private void OnValueChanged(TriProperty obj)
            {
                _property.PropertyTree.ApplyChanges();
                _actionResolver.InvokeForAllTargets(_property);
                _property.PropertyTree.Update();
            }
        }
    }
}