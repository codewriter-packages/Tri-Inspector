using System;
using Sirenix.Utilities;
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

            var type = property.ValueEntry.TypeOfValue;

            if (!type.IsDefined<DrawWithTriInspectorAttribute>() &&
                !type.Assembly.IsDefined<DrawWithTriInspectorAttribute>())
            {
                return false;
            }

            return true;
        }

        protected override void Initialize()
        {
            _serializedObject = new SerializedObject(ValueEntry.SmartValue);
            _propertyTree = new TriPropertyTreeForSerializedObject(_serializedObject);
            _propertyTree.Initialize();
        }

        public void Dispose()
        {
            _propertyTree.Dispose();
            _serializedObject.Dispose();
        }

        protected override void Validate(ValidationResult result)
        {
            _serializedObject.Update();

            _propertyTree.Update();
            _propertyTree.RunValidation();
            _propertyTree.CopyValidationResultsTo(result);
        }
    }
}