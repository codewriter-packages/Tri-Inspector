using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TriInspector.Utilities;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriPropertyCollectionVisualElement : VisualElement
    {
        private static readonly List<DeclareGroupBaseAttribute> EmptyDeclarations = new();

        private List<DeclareGroupBaseAttribute> _declarations = EmptyDeclarations;
        private Dictionary<string, TriPropertyCollectionVisualElement> _groups;

        public TriPropertyCollectionVisualElement()
        {
        }

        public TriPropertyCollectionVisualElement(Type declarationsType, IReadOnlyList<TriProperty> properties)
        {
            _declarations = GetGroupDeclarations(declarationsType);

            foreach (var property in properties)
            {
                AddProperty(property);
            }
        }

        [PublicAPI]
        public void AddProperty(TriProperty property)
        {
            AddProperty(property, default, out _);
        }

        public void AddProperty(TriProperty property, TriPropertyVisualElement.Props props, out string group)
        {
            var propertyElement = new TriPropertyVisualElement(property, props);

            if (property.TryGetAttribute(out GroupAttribute groupAttribute))
            {
                IEnumerable<string> path = groupAttribute.Path.Split('/');

                var remaining = path.GetEnumerator();
                if (remaining.MoveNext())
                {
                    group = remaining.Current;
                    AddGroupedChild(propertyElement, property, remaining.Current, remaining.Current, remaining);
                }
                else
                {
                    group = null;
                    AddPropertyChild(propertyElement, property);
                }
            }
            else
            {
                group = null;
                AddPropertyChild(propertyElement, property);
            }
        }

        protected virtual void AddGroupedChild(VisualElement child, TriProperty property,
            string currentPath, string currentName, IEnumerator<string> remainingPath)
        {
            if (_groups == null)
            {
                _groups = new Dictionary<string, TriPropertyCollectionVisualElement>();
            }

            var groupElement = CreateSubGroup(property, currentPath, currentName);

            if (remainingPath.MoveNext())
            {
                var nextPath = currentPath + "/" + remainingPath.Current;
                var nextName = remainingPath.Current;

                groupElement.AddGroupedChild(child, property, nextPath, nextName, remainingPath);
            }
            else
            {
                groupElement.AddPropertyChild(child, property);
            }
        }

        protected virtual void AddPropertyChild(VisualElement child, TriProperty property)
        {
            Add(child);
        }

        private TriPropertyCollectionVisualElement CreateSubGroup(TriProperty property,
            string groupPath, string groupName)
        {
            if (_groups.TryGetValue(groupName, out var groupElement))
            {
                return groupElement;
            }

            var declaration = _declarations.Find(it => it.Path == groupPath);

            if (declaration != null)
            {
                groupElement = TriDrawersUtilities.TryCreateGroupVisualElementFor(declaration);
            }

            if (groupElement == null)
            {
                groupElement = new TriPropertyCollectionVisualElement();
            }

            groupElement._declarations = _declarations;

            _groups.Add(groupName, groupElement);

            AddPropertyChild(groupElement, property);

            return groupElement;
        }

        private static List<DeclareGroupBaseAttribute> GetGroupDeclarations(Type type)
        {
            if (type == null)
            {
                return EmptyDeclarations;
            }

            var attrs = TriReflectionUtilities.GetAttributesCached(type);

            if (attrs.Count == 0)
            {
                return EmptyDeclarations;
            }

            List<DeclareGroupBaseAttribute> declarations = null;

            foreach (var attribute in attrs)
            {
                if (attribute is DeclareGroupBaseAttribute declareAttribute)
                {
                    declarations ??= new List<DeclareGroupBaseAttribute>();
                    declarations.Add(declareAttribute);
                }
            }

            return declarations ?? EmptyDeclarations;
        }
    }
}