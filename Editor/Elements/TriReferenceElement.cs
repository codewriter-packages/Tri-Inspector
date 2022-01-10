using System;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;

namespace TriInspector.Elements
{
    internal class TriReferenceElement : TriPropertyCollectionBaseElement
    {
        private readonly bool _drawPrefixLabel;
        private readonly bool _inline;
        private readonly float _labelWidth;
        private readonly TriProperty _property;

        private Type _referenceType;

        public TriReferenceElement(TriProperty property,
            bool inline = false, bool drawPrefixLabel = false, float labelWidth = 0f)
        {
            _property = property;
            _inline = inline;
            _drawPrefixLabel = drawPrefixLabel;
            _labelWidth = labelWidth;

            DeclareGroups(property.ValueType);
        }

        public override bool Update()
        {
            var dirty = false;

            if (_inline || _property.IsExpanded)
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

            if (_inline || _property.IsExpanded)
            {
                height += base.GetHeight(width);
            }

            return height;
        }

        public override void OnGUI(Rect position)
        {
            if (_drawPrefixLabel)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);
            }

            var headerRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            var headerLabelRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.labelWidth,
            };
            var headerFieldRect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight,
                xMin = headerRect.xMin + EditorGUIUtility.labelWidth,
            };
            var contentRect = new Rect(position)
            {
                yMin = position.yMin + headerRect.height,
            };

            if (_inline)
            {
                TriManagedReferenceGui.DrawTypeSelector(headerRect, _property);

                using (TriGuiHelper.PushLabelWidth(_labelWidth))
                {
                    base.OnGUI(contentRect);
                }
            }
            else
            {
                TriEditorGUI.Foldout(headerLabelRect, _property);
                TriManagedReferenceGui.DrawTypeSelector(headerFieldRect, _property);

                if (_property.IsExpanded)
                {
                    using (TriGuiHelper.PushIndentLevel())
                    using (TriGuiHelper.PushLabelWidth(_labelWidth))
                    {
                        base.OnGUI(contentRect);
                    }
                }
            }
        }

        private bool GenerateChildren()
        {
            if (_property.ValueType == _referenceType)
            {
                return false;
            }

            _referenceType = _property.ValueType;

            RemoveAllChildren();

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

            _referenceType = null;
            RemoveAllChildren();

            return true;
        }
    }
}