using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Validators;
using UnityEngine;

[assembly: RegisterTriAttributeValidator(typeof(RequiredGetValidator), ApplyOnArrayElement = false)]

namespace TriInspector.Validators
{
    public class RequiredGetValidator : TriAttributeValidator<RequiredGetAttribute>
    {
        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition propertyDefinition)
        {
            if (!typeof(Component).IsAssignableFrom(GetComponentType(propertyDefinition)))
            {
                return "RequiredGet attribute valid only on types derived from Component";
            }

            if (!Attribute.InChildren && !Attribute.InParents && !Attribute.IncludeSelf)
            {
                return "RequiredGet cannot be used when all InChildren,InParents,IncludeSelf is set to false";
            }

            return base.Initialize(propertyDefinition);
        }

        public override TriValidationResult Validate(TriProperty property)
        {
            for (var targetIndex = 0; targetIndex < property.PropertyTree.TargetsCount; targetIndex++)
            {
                var result = Validate(property, targetIndex);

                if (result.MessageType != TriMessageType.None)
                {
                    return result;
                }
            }

            return TriValidationResult.Valid;
        }

        private TriValidationResult Validate(TriProperty property, int targetIndex)
        {
            if (property.PropertyTree.RootProperty.GetValue(targetIndex) is not Component rootComponent)
            {
                return TriValidationResult.Error("RequiredGet attribute can be used only in MonoBehaviour components");
            }

            var expectedComponents = EnumerateExpectedComponents(property, rootComponent);

            switch (property.GetValue(targetIndex))
            {
                case IList list:
                {
                    var actualComponents = from object element in list select element as Component;

                    return expectedComponents.SequenceEqual(actualComponents)
                        ? TriValidationResult.Valid
                        : MakeErrorWithFix(property);
                }

                case Component actualComponent:
                {
                    return expectedComponents.Contains(actualComponent)
                        ? TriValidationResult.Valid
                        : MakeErrorWithFix(property);
                }

                default:
                {
                    return MakeErrorWithFix(property);
                }
            }
        }

        private IEnumerable<Component> EnumerateExpectedComponents(TriProperty property, Component root)
        {
            var componentType = GetComponentType(property.Definition);
            var selfComponent = root.GetComponent(componentType);

            if (Attribute.IncludeSelf && selfComponent)
            {
                yield return selfComponent;
            }

            if (Attribute.InChildren)
            {
                foreach (var child in root.GetComponentsInChildren(componentType))
                {
                    if (child != selfComponent)
                    {
                        yield return child;
                    }
                }
            }

            if (Attribute.InParents)
            {
                foreach (var parent in root.GetComponentsInParent(componentType))
                {
                    if (parent != selfComponent)
                    {
                        yield return parent;
                    }
                }
            }
        }

        private TriValidationResult MakeErrorWithFix(TriProperty property)
        {
            return TriValidationResult
                .Error(GetErrorMessage(property))
                .WithFix(() => Fix(property), "Get");
        }

        private string GetErrorMessage(TriProperty property)
        {
            var inSelf = Attribute.IncludeSelf;
            var inParents = Attribute.InParents;
            var inChildren = Attribute.InChildren;
            var name = GetName(property);
            var type = GetComponentType(property.Definition).Name;

            if (property.IsArray)
            {
                var content = inChildren && inParents ? $"{(inSelf ? "self and " : "")}children and parents"
                    : inChildren ? $"{(inSelf ? "self and " : "")}children"
                    : inParents ? $"{(inSelf ? "self and " : "")}parents"
                    : inSelf ? "self"
                    : "";

                return $"{name} required to contain all {type}s from {content}";
            }
            else
            {
                var content = inChildren && inParents ? $"{(inSelf ? "self or " : "")}child or parent"
                    : inChildren ? $"{(inSelf ? "self or " : "")}child"
                    : inParents ? $"{(inSelf ? "self or " : "")}parent"
                    : inSelf ? "self"
                    : "";

                return $"{name} required to contain any {type} from {content}";
            }
        }

        private void Fix(TriProperty property)
        {
            property.SetValues(targetIndex =>
            {
                var root = (Component) property.PropertyTree.RootProperty.GetValue(targetIndex);

                var expectedComponents = EnumerateExpectedComponents(property, root).ToList();

                if (property.IsArray && property.Definition.FieldType.IsArray)
                {
                    var array = Array.CreateInstance(property.Definition.ArrayElementType, expectedComponents.Count);
                    ((IList) expectedComponents).CopyTo(array, 0);
                    return array;
                }

                if (property.IsArray)
                {
                    var list = (IList) Activator.CreateInstance(property.Definition.FieldType);
                    expectedComponents.ForEach(it => list.Add(it));
                    return list;
                }

                return expectedComponents.FirstOrDefault();
            });
        }

        private static Type GetComponentType(TriPropertyDefinition propertyDefinition)
        {
            return propertyDefinition.IsArray
                ? propertyDefinition.ArrayElementType
                : propertyDefinition.FieldType;
        }

        private static string GetName(TriProperty property)
        {
            var name = property.DisplayName;
            if (string.IsNullOrEmpty(name))
            {
                name = property.RawName;
            }

            return name;
        }
    }
}