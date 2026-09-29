using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    public sealed class TriPropertyTreeForValidation : TriPropertyTree
    {
        public TriPropertyTreeForValidation(Object targetObject)
        {
            TargetObjectType = targetObject.GetType();
            TargetsCount = 1;
            TargetIsPersistent = EditorUtility.IsPersistent(targetObject);

            RootPropertyDefinition = new TriPropertyDefinition(
                memberInfo: null,
                ownerType: null,
                order: -1,
                fieldName: "ROOT",
                fieldType: TargetObjectType,
                valueGetter: (self, targetIndex) => targetObject,
                valueSetter: (self, targetIndex, value) => targetObject,
                attributes: Array.Empty<Attribute>(),
                isArrayElement: false);

            RootProperty = new TriProperty(this, null, RootPropertyDefinition, null);
            DisableModifications = true;
        }

        public override void ForceCreateUndoGroup()
        {
        }
    }
}