using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriValidationResultsVisualElement : VisualElement
    {
        private readonly TriProperty _property;
        private readonly VisualElement _child;

        private IReadOnlyList<TriValidationResult> _cachedResults;

        public TriValidationResultsVisualElement(TriProperty property, VisualElement child)
        {
            _property = property;
            _child = child;

            Add(child);

            Rebuild();
            this.PeriodicRun(Rebuild);
        }

        private void Rebuild()
        {
            if (ReferenceEquals(_property.ValidationResults, _cachedResults))
            {
                return;
            }

            _cachedResults = _property.ValidationResults;

            for (var i = childCount - 1; i >= 0; i--)
            {
                if (this[i] != _child)
                {
                    RemoveAt(i);
                }
            }

            for (var i = 0; i < _cachedResults.Count; i++)
            {
                Insert(i, CreateResultElement(_property, _cachedResults[i]));
            }
        }

        private VisualElement CreateResultElement(TriProperty property, TriValidationResult result)
        {
            return new TriInfoBoxVisualElement(
                result.Message,
                result.MessageType,
                result.FixAction != null ? () => ExecuteFix(property, result.FixAction) : null,
                result.FixActionContent?.text);
        }

        private void ExecuteFix(TriProperty property, Action fixAction)
        {
            property.ModifyAndRecordForUndo(targetIndex => fixAction?.Invoke());
        }
    }
}
