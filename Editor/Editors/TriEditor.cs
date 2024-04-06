using UnityEditor;
using UnityEngine.UIElements;

namespace TriInspector.Editors
{
    public abstract class TriEditor : Editor
    {
        private TriEditorCore _core;

        protected virtual void OnEnable()
        {
            _core = new TriEditorCore(this);
        }

        protected virtual void OnDisable()
        {
            _core.Dispose();
        }


        public override void OnInspectorGUI()
        {
            _core.OnInspectorGUI();
        }

        public override VisualElement CreateInspectorGUI()
        {
            return _core.CreateVisualElement();
        }
    }
}