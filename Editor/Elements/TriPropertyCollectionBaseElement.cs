using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TriInspector.Utilities;

namespace TriInspector.Elements
{
    public abstract class TriPropertyCollectionBaseElement : TriElement
    {
        private List<DeclareGroupBaseAttribute> _declarations = new List<DeclareGroupBaseAttribute>();

        private Dictionary<string, TriPropertyCollectionBaseElement> _groups;

        [PublicAPI]
        public void DeclareGroups([CanBeNull] Type type)
        {
            if (type == null)
            {
                return;
            }

            foreach (var attribute in TriReflectionUtilities.GetAttributesCached(type))
            {
                if (attribute is DeclareGroupBaseAttribute declareAttribute)
                {
                    _declarations.Add(declareAttribute);
                }
            }
        }

        [PublicAPI]
        public void AddProperty(TriProperty property)
        {
            var propertyElement = new TriPropertyElement(property);

            if (property.TryGetAttribute(out GroupAttribute groupAttribute))
            {
                IEnumerable<string> path = groupAttribute.Path.Split('/');

                var remaining = path.GetEnumerator();
                if (remaining.MoveNext())
                {
                    AddGroupedChild(propertyElement, remaining.Current, remaining.Current, remaining);
                }
                else
                {
                    AddChild(propertyElement);
                }
            }
            else
            {
                AddChild(propertyElement);
            }
        }

        private void AddGroupedChild(TriElement child, string currentPath, string currentName,
            IEnumerator<string> remainingPath)
        {
            if (_groups == null)
            {
                _groups = new Dictionary<string, TriPropertyCollectionBaseElement>();
            }

            var groupElement = CreateSubGroup(currentPath, currentName);

            if (remainingPath.MoveNext())
            {
                var nextPath = currentPath + "/" + remainingPath.Current;
                var nextName = remainingPath.Current;

                groupElement.AddGroupedChild(child, nextPath, nextName, remainingPath);
            }
            else
            {
                groupElement.AddChild(child);
            }
        }

        private TriPropertyCollectionBaseElement CreateSubGroup(string groupPath, string groupName)
        {
            if (!_groups.TryGetValue(groupName, out var groupElement))
            {
                var declaration = _declarations.FirstOrDefault(it => it.Path == groupPath);

                if (declaration != null)
                {
                    groupElement = TriDrawersUtilities.TryCreateGroupElementFor(declaration);
                }

                if (groupElement == null)
                {
                    groupElement = new DefaultGroupElement();
                }

                groupElement._declarations = _declarations;

                _groups.Add(groupName, groupElement);

                AddChild(groupElement);
            }

            return groupElement;
        }

        private class DefaultGroupElement : TriPropertyCollectionBaseElement
        {
        }
    }
}