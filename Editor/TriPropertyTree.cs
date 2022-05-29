using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TriInspector.Elements;
using UnityEditor;
using UnityEngine;

namespace TriInspector
{
    public abstract class TriPropertyTree : ITriPropertyParent
    {
        [CanBeNull] private TriInspectorElement _inspectorElement;

        public IReadOnlyList<TriProperty> Properties { get; protected set; }

        public Type TargetObjectType { get; protected set; }
        public int TargetsCount { get; protected set; }
        public bool TargetIsPersistent { get; protected set; }

        public bool ValidationRequired { get; private set; } = true;
        public bool RepaintRequired { get; private set; } = true;

        public int RepaintFrame { get; private set; } = 0;

        public virtual void Dispose()
        {
            if (_inspectorElement != null && _inspectorElement.IsAttached)
            {
                _inspectorElement.DetachInternal();
            }
        }

        public virtual void Update()
        {
            RepaintFrame++;
        }

        public void RunValidation()
        {
            ValidationRequired = false;

            foreach (var property in Properties)
            {
                property.RunValidation();
            }

            RequestRepaint();
        }

        public void Draw()
        {
            RepaintRequired = false;

            if (_inspectorElement == null)
            {
                _inspectorElement = new TriInspectorElement(TargetObjectType, Properties);
                _inspectorElement.AttachInternal();
            }

            _inspectorElement.Update();
            var width = EditorGUIUtility.currentViewWidth;
            var height = _inspectorElement.GetHeight(width);
            var rect = GUILayoutUtility.GetRect(width, height);
            _inspectorElement.OnGUI(rect);
        }

        public void EnumerateValidationResults(Action<TriProperty, TriValidationResult> call)
        {
            foreach (var triProperty in Properties)
            {
                triProperty.EnumerateValidationResults(call);
            }
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

        public abstract object GetValue(int targetIndex);
        public abstract void NotifyValueChanged(TriProperty property);
        public abstract void ForceCreateUndoGroup();
        public abstract void PrepareForValueModification();
        public abstract void UpdateAfterValueModification();
    }
}