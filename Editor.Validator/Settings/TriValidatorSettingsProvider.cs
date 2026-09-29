using TriInspector.Editors;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Validator
{
    internal sealed class TriValidatorSettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedObject;
        private TriEditorCore _core;

        private TriValidatorSettingsProvider()
            : base("Project/Tri Inspector/Validator", SettingsScope.Project)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = TriValidatorSettings.instance;

            // ScriptableSingleton is created NotEditable by default, which would silently discard edits.
            settings.hideFlags &= ~HideFlags.NotEditable;

            _serializedObject = new SerializedObject(settings);
            _core = new TriEditorCore(_serializedObject)
            {
                HideMonoScript = true,
            };

            var inspector = new VisualElement();
            inspector.AddToClassList(TriStyles.UnityInspectorElement);
            inspector.AddToClassList(TriStyles.UnityInspectorMainContainer);
            inspector.style.paddingLeft = 10f;
            inspector.style.paddingRight = 10f;
            inspector.Add(_core.CreateVisualElement());
            rootElement.Add(inspector);

            rootElement.TrackSerializedObjectValue(_serializedObject, _ => TriValidatorSettings.instance.Save());
        }

        public override void OnDeactivate()
        {
            if (_serializedObject != null)
            {
                _serializedObject.ApplyModifiedProperties();
                TriValidatorSettings.instance.Save();
            }

            _core?.Dispose();
            _core = null;

            _serializedObject?.Dispose();
            _serializedObject = null;
        }

        [SettingsProvider]
        public static SettingsProvider CreateTriValidatorSettingsProvider()
        {
            return new TriValidatorSettingsProvider
            {
                keywords = new[] {"Tri", "Validator", "Validation", "Folders", "Included", "Excluded", "Scope"},
            };
        }
    }
}