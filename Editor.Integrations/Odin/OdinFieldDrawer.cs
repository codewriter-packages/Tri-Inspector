using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using TriInspector.Utilities;
using UnityEngine;

namespace TriInspector.Editor.Integrations.Odin
{
    [DrawerPriority(0.0, 10000.0, 1.0)]
    public class OdinFieldDrawer<T> : OdinValueDrawer<T>, IDisposable
    {
        private TriPropertyTree _propertyTree;
        private LabelOverrideContext _labelOverrideContext;

        public override bool CanDrawTypeFilter(Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return false;
            }

            if (!type.IsDefined<DrawWithTriInspectorAttribute>() &&
                !type.Assembly.IsDefined<DrawWithTriInspectorAttribute>())
            {
                return false;
            }

            return true;
        }

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            if (property.IsTreeRoot)
            {
                return false;
            }

            for (var parent = property.Parent; parent != null; parent = parent.Parent)
            {
                var parentType = parent.ValueEntry.TypeOfValue;
                if (parentType.IsDefined<DrawWithTriInspectorAttribute>() ||
                    parentType.Assembly.IsDefined<DrawWithTriInspectorAttribute>())
                {
                    return false;
                }
            }

            return true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _propertyTree = new TriPropertyTreeForOdin<T>(ValueEntry);
            _labelOverrideContext = new LabelOverrideContext(_propertyTree);
        }

        public void Dispose()
        {
            _propertyTree?.Dispose();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            _propertyTree.Update();

            if (_propertyTree.ValidationRequired)
            {
                _propertyTree.RunValidation();
            }

            _labelOverrideContext.Label = label ?? GUIContent.none;

            using (TriPropertyOverrideContext.BeginOverride(_labelOverrideContext))
            {
                _propertyTree.Draw();
            }
        }

        private class LabelOverrideContext : TriPropertyOverrideContext
        {
            private readonly TriPropertyTree _tree;

            public LabelOverrideContext(TriPropertyTree tree)
            {
                _tree = tree;
            }

            public GUIContent Label { get; set; }

            public override bool TryGetDisplayName(TriProperty property, out GUIContent displayName)
            {
                if (property == _tree.RootProperty)
                {
                    displayName = Label;
                    return true;
                }

                displayName = default;
                return false;
            }
        }
    }
}