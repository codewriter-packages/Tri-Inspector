using System;
using TriInspector.Elements;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    public abstract class TriPropertyTree
    {
        private TriPropertyElement _rootPropertyElement;

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

        public void RunValidation()
        {
            ValidationRequired = false;

            RootProperty.RunValidation();

            RequestRepaint();
        }

        public void Draw(float? viewWidth = null)
        {
            RepaintRequired = false;

            if (_rootPropertyElement == null)
            {
                _rootPropertyElement = new TriPropertyElement(RootProperty, new TriPropertyElement.Props
                {
                    forceInline = RootProperty.MemberInfo == null,
                });
                _rootPropertyElement.AttachInternal();
            }

            _rootPropertyElement.Update();
            var width = viewWidth ?? EditorGUIUtility.currentViewWidth;
            var height = _rootPropertyElement.GetHeight(width);
            var rect = GUILayoutUtility.GetRect(width, height);
            rect.xMin += 3;
            _rootPropertyElement.OnGUI(rect);
        }

        public void EnumerateValidationResults(Action<TriProperty, TriValidationResult> call)
        {
            RootProperty.EnumerateValidationResults(call);
        }

        public virtual void RequestRepaint()
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