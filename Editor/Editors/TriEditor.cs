using UnityEditor;
using UnityEngine.UIElements;

namespace TriInspector.Editors
{
    public abstract class TriEditor : Editor
    {
        private TriEditorCore _core;

        private void OnEnable()
        {
            _core = new TriEditorCore(this);
        }

        private void OnDisable()
        {
            _core.Dispose();
        }

        public override VisualElement CreateInspectorGUI()
        {
            return _core.CreateVisualElement();
        }
    }
}