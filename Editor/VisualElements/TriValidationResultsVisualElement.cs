using System;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriValidationResultsVisualElement : VisualElement
    {
        private readonly TriProperty _property;
        private readonly VisualElement _child;
        private readonly VisualElement _bg;

        public TriValidationResultsVisualElement(TriProperty property, VisualElement child)
        {
            _property = property;
            _child = child;

            _bg = new VisualElement();
            _bg.AddToClassList(TriStyles.TriValidationResultsBg);
            Add(_bg);

            Add(child);

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                property.ValidationResultsChanged += OnValidationResultsChanged;
                Rebuild();
            });
            RegisterCallback<DetachFromPanelEvent>(_ => property.ValidationResultsChanged -= OnValidationResultsChanged);
        }

        private void OnValidationResultsChanged(TriProperty _) => Rebuild();

        private void Rebuild()
        {
            var results = _property.ValidationResults;

            for (var i = childCount - 1; i >= 0; i--)
            {
                if (this[i] != _child && this[i] != _bg)
                {
                    RemoveAt(i);
                }
            }

            _bg.style.display = results.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            EnableInClassList(TriStyles.TriValidationResults, results.Count > 0);

            var messageType = GetHighestErrorType(results);
            _bg.EnableInClassList(TriStyles.InfoBoxInfo, messageType == TriMessageType.Info);
            _bg.EnableInClassList(TriStyles.InfoBoxWarning, messageType == TriMessageType.Warning);
            _bg.EnableInClassList(TriStyles.InfoBoxError, messageType == TriMessageType.Error);

            for (var i = 0; i < results.Count; i++)
            {
                Insert(i + 1, CreateResultElement(i, _property, results[i]));
            }
        }

        private VisualElement CreateResultElement(int index, TriProperty property, TriValidationResult result)
        {
            var el = new TriInfoBoxVisualElement(
                result.Message,
                result.MessageType,
                result.FixAction != null ? () => ExecuteFix(property, result.FixAction) : null,
                result.FixActionContent?.text);

            el.EnableInClassList(TriStyles.InfoBoxFirst, index == 0);

            return el;
        }

        private void ExecuteFix(TriProperty property, Action fixAction)
        {
            property.ModifyAndRecordForUndo(targetIndex => fixAction?.Invoke());
        }

        private static TriMessageType GetHighestErrorType(TriArray<TriValidationResult> results)
        {
            var highest = TriMessageType.None;

            foreach (var it in results)
            {
                if (it.MessageType > highest)
                {
                    highest = it.MessageType;
                }
            }

            return highest;
        }
    }
}