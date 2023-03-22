using System;
using TriInspector.Elements;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    public abstract class TriPropertyTree
    {
        private TriPropertyElement _rootPropertyElement;
        private Rect _cachedOuterRect = new Rect(0, 0, 0, 0);

        public TriPropertyDefinition RootPropertyDefinition { get; protected set; }
        public TriProperty RootProperty { get; protected set; }

        public Type TargetObjectType { get; protected set; }
        public int TargetsCount { get; protected set; }
        public bool TargetIsPersistent { get; protected set; }

        public bool ValidationRequired { get; private set; } = true;
        public bool RepaintRequired { get; private set; } = true;

        public int RepaintFrame { get; private set; } = 0;

        public virtual void Dispose()
        {
            if (_rootPropertyElement != null && _rootPropertyElement.IsAttached)
            {
                _rootPropertyElement.DetachInternal();
            }
        }

        public virtual void Update(bool forceUpdate = false)
        {
            RepaintFrame++;
        }

        public virtual bool ApplyChanges()
        {
            return false;
        }

        public void RunValidationIfRequired()
        {
            if (!ValidationRequired)
            {
                return;
            }
            
            RunValidation();
        }

        public void RunValidation()
        {
            ValidationRequired = false;

            RootProperty.RunValidation();

            RequestRepaint();
        }

        public virtual void Draw()
        {
            RepaintRequired = false;

            if (_rootPropertyElement == null)
            {
                _rootPropertyElement = new TriPropertyElement(RootProperty, new TriPropertyElement.Props
                {
                    forceInline = !RootProperty.TryGetMemberInfo(out _),
                });
                _rootPropertyElement.AttachInternal();
            }

            _rootPropertyElement.Update();

            var rectOuter = GUILayoutUtility.GetRect(0, 9999, 0, 0);
            _cachedOuterRect = Event.current.type == EventType.Layout ? _cachedOuterRect : rectOuter;

            var rect = new Rect(_cachedOuterRect);
            rect = EditorGUI.IndentedRect(rect);
            rect.height = _rootPropertyElement.GetHeight(rect.width);

            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            GUILayoutUtility.GetRect(_cachedOuterRect.width, rect.height);

            _rootPropertyElement.OnGUI(rect);

            EditorGUI.indentLevel = oldIndent;
        }

        public void EnumerateValidationResults(Action<TriProperty, TriValidationResult> call)
        {
            RootProperty.EnumerateValidationResults(call);
        }

        public void RequestRepaint()
        {
            RepaintRequired = true;
        }

        public void RequestValidation()
        {
            ValidationRequired = true;

            RequestRepaint();
        }

        public abstract void ForceCreateUndoGroup();
    }
}