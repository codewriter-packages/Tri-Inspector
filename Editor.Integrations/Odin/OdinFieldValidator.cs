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
        private TriPropertyTreeForOdin<T> _propertyTree;

        public override RevalidationCriteria RevalidationCriteria { get; }
            = RevalidationCriteria.OnValueChangeOrChildValueChange;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            if (property.IsTreeRoot)
            {
                return false;
            }

            var type = property.ValueEntry.TypeOfValue;

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
                var parentType = parent.ValueEntry.TypeOfValue;
                if (parentType.IsDefined<DrawWithTriInspectorAttribute>() ||
                    parentType.Assembly.IsDefined<DrawWithTriInspectorAttribute>())
                {
                    return false;
                }
            }

            return true;
        }

        protected override void Initialize()
        {
            _propertyTree = new TriPropertyTreeForOdin<T>(ValueEntry);
        }

        public void Dispose()
        {
            _propertyTree.Dispose();
        }

        protected override void Validate(ValidationResult result)
        {
            _propertyTree.Update();
            _propertyTree.RunValidation();
            _propertyTree.CopyValidationResultsTo(result);
        }
    }
}