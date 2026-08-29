using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector.Editors
{
    public class TriEditorCore
    {
        private readonly Editor _editor;

        private TriPropertyTreeForSerializedObject _inspector;

        public TriEditorCore(Editor editor)
        {
            _editor = editor;
        }

        public void Dispose()
        {
            if (_inspector != null)
            {
                _inspector.Dispose();
            }

            _inspector = null;
        }

        public VisualElement CreateVisualElement()
        {
            var serializedObject = _editor.serializedObject;

            var container = new VisualElement();

            if (serializedObject.targetObjects.Length == 0 || serializedObject.targetObject == null)
            {
                container.Add(new HelpBox("Script is missing", HelpBoxMessageType.Warning));
                return container;
            }

            if (_inspector == null)
            {
                _inspector = new TriPropertyTreeForSerializedObject(serializedObject);
            }
            
            serializedObject.UpdateIfRequiredOrScript();
            _inspector.Update();
            _inspector.RunValidation();

            if (!_inspector.RootProperty.TryGetAttribute(out HideMonoScriptAttribute _))
            {
                var scriptProperty = serializedObject.FindProperty("m_Script");
                if (scriptProperty != null)
                {
                    var scriptField = new PropertyField(scriptProperty);
                    scriptField.SetEnabled(false);
                    scriptField.Bind(serializedObject);
                    container.Add(scriptField);
                }
            }

            container.Add(_inspector.GetRootElement());

            container.schedule.Execute(() =>
            {
                _inspector.Update();
                _inspector.RunValidationIfRequired();
            }).Every(0);

            return container;
        }
    }
}