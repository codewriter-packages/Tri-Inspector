using TriInspector.Editors;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace TriInspector
{
    [HideMonoScript]
    public abstract class TriEditorWindow : EditorWindow
    {
        private SerializedObject _serializedObject;
        private TriEditorCore _core;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
            _core = new TriEditorCore(_serializedObject);
        }

        private void OnDisable()
        {
            _core.Dispose();
            _serializedObject.Dispose();
        }

        private void CreateGUI()
        {
            var inspector = new VisualElement();
            inspector.AddToClassList(TriStyles.UnityInspectorElement);
            inspector.AddToClassList(TriStyles.UnityInspectorMainContainer);
            inspector.Add(_core.CreateVisualElement());
            rootVisualElement.Add(inspector);
        }
    }
}