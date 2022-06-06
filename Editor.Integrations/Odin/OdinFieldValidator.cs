using System;
using Sirenix.Utilities;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using TriInspector.Editor.Integrations.Odin;

[assembly: RegisterValidator(typeof(OdinFieldValidator<>))]

namespace TriInspector.Editor.Integrations.Odin
{
    public class OdinFieldValidator<T> : ValueValidator<T>, IDisposable
    {
        private bool _initialized;
        private TriPropertyTreeForOdin<T> _propertyTree;

        public override RevalidationCriteria RevalidationCriteria { get; }
            = RevalidationCriteria.OnValueChangeOrChildValueChange;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            if (property.IsTreeRoot)
            {
                return false;
            }

            var type = property.Info.TypeOfValue;

            if (type == null)
            {
                return false;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return false;
            }

            if (!type.IsDefined<DrawWithTriInspectorAttribute>() &&
                !type.Assembly.IsDefined<DrawWithTriInspectorAttribute>())
            {
                return false;
            }

            for (var parent = property.Parent; parent != null; parent = parent.Parent)
            {
                var parentType = parent.Info.TypeOfValue;
                if (parentType.IsDefined<DrawWithTriInspectorAttribute>() ||
                    parentType.Assembly.IsDefined<DrawWithTriInspectorAttribute>())
                {
                    return false;
                }
            }

            return true;
        }

        public void Dispose()
        {
            _propertyTree?.Dispose();
        }

        protected override void Validate(ValidationResult result)
        {
            if (!_initialized)
            {
                _initialized = true;
                _propertyTree = new TriPropertyTreeForOdin<T>(ValueEntry);
            }

            _propertyTree.Update();
            _propertyTree.RunValidation();
            _propertyTree.CopyValidationResultsTo(result);
        }
    }
}