using System;
using TriInspector.Utilities;
using TriInspector.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Elements
{
    internal class TriReferenceElement : TriPropertyCollectionBaseElement
    {
        private readonly Props _props;
        private readonly TriProperty _property;
        private readonly bool _showReferencePicker;
        private readonly bool _skipReferencePickerExtraLine;

        private Type _referenceType;

        [Serializable]
        public struct Props
        {
            public bool inline;
            public bool drawPrefixLabel;
            public float labelWidth;
        }

        public TriReferenceElement(TriProperty property, Props props = default)
        {
            _property = property;
            _props = props;
            _showReferencePicker = !property.TryGetAttribute(out HideReferencePickerAttribute _);
            _skipReferencePickerExtraLine = !_showReferencePicker && _props.inline;
        }

        public override VisualElement CreateVisualElement(TriProperty property)
        {
            var content = new VisualElement();
            var builtType = default(Type);
            var hasBuilt = false;

            void BuildChildren()
            {
                hasBuilt = true;
                builtType = _property.ValueType;

                content.Clear();
                GenerateChildren();
                content.Add(CreateChildrenColumn(property));
            }

            void OnValueChanged(TriProperty changed)
            {
                if (hasBuilt && _property.ValueType != builtType)
                {
                    BuildChildren();
                }
            }

            void BindLifecycle(VisualElement root)
            {
                root.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    _property.ValueChanged += OnValueChanged;
                    OnValueChanged(_property);
                });
                root.RegisterCallback<DetachFromPanelEvent>(_ => _property.ValueChanged -= OnValueChanged);
            }

            if (_props.inline)
            {
                var column = new VisualElement();

                if (_showReferencePicker)
                {
                    column.Add(CreateTypeSelectorIsland());
                }

                BuildChildren();
                column.Add(content);

                var inlineRoot = _props.drawPrefixLabel ? new TriAlignedLabel(_property.DisplayName, column) : column;
                BindLifecycle(inlineRoot);
                return inlineRoot;
            }

            var foldout = new Foldout
            {
                text = _property.DisplayName,
                value = _property.IsExpanded,
            };

            if (_showReferencePicker)
            {
                var toggle = foldout.Q<Toggle>();
                if (toggle != null)
                {
                    var typeContainer = new TriAlignedLabel(" ", CreateTypeSelectorIsland())
                    {
                        style =
                        {
                            position = Position.Absolute,
                            left = 0,
                            right = 0,
                            top = 0,
                            bottom = 0,
                        },
                    };

                    toggle.Add(typeContainer);
                }
            }

            if (_property.IsExpanded)
            {
                BuildChildren();
            }

            foldout.RegisterValueChangedCallback(evt =>
            {
                // Foldout also bubbles ChangeEvent<bool> from child toggles; only react to its own.
                if (evt.target != foldout)
                {
                    return;
                }

                _property.IsExpanded = evt.newValue;

                if (evt.newValue && !hasBuilt)
                {
                    BuildChildren();
                }
            });

            foldout.Add(content);

            BindLifecycle(foldout);

            return foldout;
        }

        private IMGUIContainer CreateTypeSelectorIsland()
        {
            // The managed-reference picker is an IMGUI AdvancedDropdown with no native equivalent
            return new IMGUIContainer(() =>
            {
                var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                TriManagedReferenceGui.DrawTypeSelector(rect, _property);
            });
        }

        public override bool Update()
        {
            var dirty = false;

            if (_props.inline || _property.IsExpanded)
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
            var height = _skipReferencePickerExtraLine ? 0f : EditorGUIUtility.singleLineHeight;

            if (_props.inline || _property.IsExpanded)
            {
                height += base.GetHeight(width);
            }

            return height;
        }

        public override void OnGUI(Rect position)
        {
            if (_props.drawPrefixLabel)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                position = EditorGUI.PrefixLabel(position, controlId, _property.DisplayNameContent);
            }

            var headerRect = new Rect(position)
            {
                height = _skipReferencePickerExtraLine ? 0f : EditorGUIUtility.singleLineHeight,
            };
            var headerLabelRect = new Rect(position)
            {
                height = headerRect.height,
                width = EditorGUIUtility.labelWidth,
            };
            var headerFieldRect = new Rect(position)
            {
                height = headerRect.height,
                xMin = headerRect.xMin + EditorGUIUtility.labelWidth,
            };
            var contentRect = new Rect(position)
            {
                yMin = position.yMin + headerRect.height,
            };

            if (_props.inline)
            {
                if (_showReferencePicker)
                {
                    TriManagedReferenceGui.DrawTypeSelector(headerRect, _property);
                }

                using (TriGuiHelper.PushLabelWidth(_props.labelWidth))
                {
                    base.OnGUI(contentRect);
                }
            }
            else
            {
                TriEditorGUI.Foldout(headerLabelRect, _property);

                if (_showReferencePicker)
                {
                    TriManagedReferenceGui.DrawTypeSelector(headerFieldRect, _property);
                }

                if (_property.IsExpanded)
                {
                    using (var indentedRectScope = TriGuiHelper.PushIndentedRect(contentRect, 1))
                    using (TriGuiHelper.PushLabelWidth(_props.labelWidth))
                    {
                        base.OnGUI(indentedRectScope.IndentedRect);
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

            ClearGroups();
            DeclareGroups(_property.ValueType);

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