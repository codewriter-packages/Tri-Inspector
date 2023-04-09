﻿using System.Linq;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    internal class TriFoldoutElement : TriPropertyCollectionBaseElement
    {
        private readonly TriProperty _property;

        private readonly bool _grouped;

        public TriFoldoutElement(TriProperty property)
        {
            _property = property;
            
            var prop = _property;
            while (!_grouped && prop.Parent != null)
            {
                _grouped = prop.TryGetAttribute(out GroupAttribute _);

                prop = prop.Parent;
            }
            
            DeclareGroups(property.ValueType);
        }

        public override bool Update()
        {
            var dirty = false;

            if (_property.IsExpanded)
            {
                dirty |= GenerateChildren();
            }
            else
            {
                dirty |= ClearChildren();
            }

            dirty |= base.Update();

            return dirty;
        }

        public override float GetHeight(float width)
        {
            var height = EditorGUIUtility.singleLineHeight;

            if (!_property.IsExpanded)
            {
                return height;
            }

            height += base.GetHeight(width);

            return height;
        }

        public override void OnGUI(Rect position)
        {
            var headerRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            var contentRect = new Rect(position)
            {
                yMin = position.yMin + headerRect.height,
            };

            if (_grouped)
            {
                headerRect.x += 12;
                headerRect.width -= 12;
            }
            
            TriEditorGUI.Foldout(headerRect, _property);

            if (!_property.IsExpanded)
            {
                return;
            }

            using (var indentedRectScope = TriGuiHelper.PushIndentedRect(contentRect, 1))
            {
                base.OnGUI(indentedRectScope.IndentedRect);
            }
        }

        private bool GenerateChildren()
        {
            if (ChildrenCount != 0)
            {
                return false;
            }

            foreach (var childProperty in _property.ChildrenProperties)
            {
                AddProperty(childProperty);
            }

            return true;
        }

        private bool ClearChildren()
        {
            if (ChildrenCount == 0)
            {
                return false;
            }

            RemoveAllChildren();

            return true;
        }
    }
}