using System;
using Sirenix.Utilities;
using Sirenix.OdinInspector.Editor;
using TriInspector.Utilities;
using UnityEngine;

namespace TriInspector.Editor.Integrations.Odin
{
    [DrawerPriority(0.0, 10000.0, 1.0)]
    public class OdinObjectDrawer<T> : OdinValueDrawer<T>, IDisposable
        where T : UnityEngine.Object
    {
        private TriPropertyTree _propertyTree;

        public override bool CanDrawTypeFilter(Type type)
        {
            if (!type.IsDefined<DrawWithTriInspectorAttribute>() &&
                !type.Assembly.IsDefined<DrawWithTriInspectorAttribute>())
            {
                return false;
            }

            return true;
        }

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            if (!property.IsTreeRoot)
            {
                return false;
            }

            if (property.Tree.UnitySerializedObject == null)
            {
                return false;
            }

            return true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            var serializedObject = Property.Tree.UnitySerializedObject;
            _propertyTree = new TriPropertyTreeForSerializedObject(serializedObject);
            _propertyTree.Initialize();
        }

        public void Dispose()
        {
            _propertyTree?.Dispose();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (TriGuiHelper.IsEditorTargetPushed(ValueEntry.SmartValue))
            {
                GUILayout.Label("Recursive inline editors not supported");
                return;
            }

            _propertyTree.Update();

            if (_propertyTree.ValidationRequired)
            {
                _propertyTree.RunValidation();
            }

            using (TriGuiHelper.PushEditorTarget(ValueEntry.SmartValue))
            {
                _propertyTree.Draw();
            }
        }
    }
}