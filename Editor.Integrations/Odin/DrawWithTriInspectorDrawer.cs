using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Editor.Integrations.Odin
{
    [DrawerPriority(0.0, 0.0, 6000.0)]
    public class DrawWithTriInspectorDrawer<T> : OdinAttributeDrawer<DrawWithTriInspectorAttribute, T>, IDisposable
    {
        private TriPropertyTreeForOdin<T> _propertyTree;

        protected override void Initialize()
        {
            base.Initialize();

            _propertyTree = new TriPropertyTreeForOdin<T>(ValueEntry);
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
                EditorGUI.indentLevel++;
                _propertyTree.Draw();
                EditorGUI.indentLevel--;
            }
        }
    }
}