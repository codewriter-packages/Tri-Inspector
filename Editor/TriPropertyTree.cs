using System;
using System.Collections.Generic;
using TriInspector.Elements;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace TriInspector
{
    public abstract class TriPropertyTree : IDisposable
    {
        private readonly List<TriPropertyOverrideContext> _propertyOverrides =
            new List<TriPropertyOverrideContext>();

        private TriPropertyElement _rootPropertyElement;

        public TriPropertyDefinition RootPropertyDefinition { get; protected set; }
        public TriProperty RootProperty { get; protected set; }

        public Type TargetObjectType { get; protected set; }
        public int TargetsCount { get; protected set; }
        public bool TargetIsPersistent { get; protected set; }

        public bool ValidationRequired { get; private set; } = true;

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

            Profiler.BeginSample("TriInspector.RunValidation");
            RootProperty.RunValidation();
            Profiler.EndSample();

            RequestRepaint();
        }

        public ITriElement GetRootElement()
        {
            if (_rootPropertyElement == null)
            {
                _rootPropertyElement = new TriPropertyElement(RootProperty, new TriPropertyElement.Props
                {
                    forceInline = !RootProperty.TryGetMemberInfo(out _),
                });
            }

            return _rootPropertyElement;
        }

        public void EnumerateValidationResults(Action<TriProperty, TriValidationResult> call)
        {
            RootProperty.EnumerateValidationResults(call);
        }

        [Obsolete("Legacy from IMGUI")]
        public void RequestRepaint()
        {
        }

        public void RequestValidation()
        {
            ValidationRequired = true;
        }

        public abstract void ForceCreateUndoGroup();

        public void AddPropertyOverride(TriPropertyOverrideContext context)
        {
            _propertyOverrides.Add(context);
        }

        public void RemovePropertyOverride(TriPropertyOverrideContext context)
        {
            _propertyOverrides.Remove(context);
        }

        internal bool TryGetOverrideDisplayName(TriProperty property, out GUIContent displayName)
        {
            for (var i = _propertyOverrides.Count - 1; i >= 0; i--)
            {
                if (_propertyOverrides[i].TryGetDisplayName(property, out displayName))
                {
                    return true;
                }
            }

            displayName = default;
            return false;
        }
    }
}