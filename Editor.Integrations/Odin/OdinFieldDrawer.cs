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

            return true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _propertyTree = new TriPropertyTreeForOdin<T>(ValueEntry);
            _propertyTree.Initialize(TriEditorMode.None);
        }

        public void Dispose()
        {
            _propertyTree?.Dispose();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var propertyState = ValueEntry.Property.State;

            propertyState.Expanded = SirenixEditorGUI.Foldout(propertyState.Expanded, label);

            if (propertyState.Expanded)
            {
                using (TriGuiHelper.PushIndentLevel())
                {
                    _propertyTree.Update();

                    if (_propertyTree.ValidationRequired)
                    {
                        _propertyTree.RunValidation();
                    }

                    _propertyTree.Draw();
                }
            }
        }
    }
}