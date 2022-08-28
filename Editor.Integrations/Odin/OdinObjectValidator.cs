using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using TriInspector.Editor.Integrations.Odin;
using UnityEditor;

[assembly: RegisterValidator(typeof(OdinObjectValidator<>))]

namespace TriInspector.Editor.Integrations.Odin
{
    public class OdinObjectValidator<T> : ValueValidator<T>, IDisposable
        where T : UnityEngine.Object
    {
        private bool _initialized;
        private TriPropertyTreeForSerializedObject _propertyTree;
        private SerializedObject _serializedObject;

        public override RevalidationCriteria RevalidationCriteria { get; }
            = RevalidationCriteria.OnValueChangeOrChildValueChange;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            if (!property.IsTreeRoot)
            {
                return false;
            }

            var type = property.Info.TypeOfValue;

            if (type == null)
            {
                return false;
            }

            if (!TriOdinUtility.IsDrawnByTri(type))
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _propertyTree?.Dispose();
            _serializedObject?.Dispose();
        }

        protected override void Validate(ValidationResult result)
        {
            if (!_initialized)
            {
                _initialized = true;
                _serializedObject = new SerializedObject(ValueEntry.SmartValue);
                _propertyTree = new TriPropertyTreeForSerializedObject(_serializedObject);
            }

            _serializedObject.Update();

            _propertyTree.Update();
            _propertyTree.RunValidation();
            _propertyTree.CopyValidationResultsTo(result);
        }
    }
}