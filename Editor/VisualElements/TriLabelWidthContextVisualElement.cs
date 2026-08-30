using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriLabelWidthContextVisualElement : VisualElement
    {
        public float? LabelWidth { get; }

        public TriLabelWidthContextVisualElement(float? labelWidth, VisualElement child = null)
        {
            LabelWidth = labelWidth > 0 ? labelWidth : null;

            if (child != null)
            {
                Add(child);
            }
        }

        public static void SetupAlignedLabel(VisualElement field)
        {
            VisualElement inspectorElement = null;
            VisualElement contextWidthElement = null;
            var explicitWidth = false;

            void Align()
            {
                AlignLabel(field, inspectorElement, contextWidthElement, explicitWidth);
            }

            field.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                inspectorElement = null;
                contextWidthElement = null;

                for (var current = field.parent; current != null; current = current.parent)
                {
                    if (current.ClassListContains(TriStyles.UnityInspectorElement))
                    {
                        inspectorElement = current;
                    }

                    if (current.ClassListContains(TriStyles.UnityInspectorMainContainer))
                    {
                        contextWidthElement = current;
                        break;
                    }
                }

                explicitWidth = TryApplyExplicitWidth(field);

                Align();
            });

            field.RegisterCallback<CustomStyleResolvedEvent>(_ => Align());
            field.RegisterCallback<GeometryChangedEvent>(_ => Align());
        }

        private static bool TryApplyExplicitWidth(VisualElement field)
        {
            if (field.FindAncestor<TriLabelWidthContextVisualElement>() is not { } context)
            {
                return false;
            }

            if (context.LabelWidth is not { } customLabelWidth)
            {
                return false;
            }

            if (field.Q<Label>(className: BaseField<object>.labelUssClassName) is not { } label)
            {
                return false;
            }

            label.style.width = label.style.minWidth = customLabelWidth;
            return true;
        }

        private static void AlignLabel(VisualElement field, VisualElement inspectorElement,
            VisualElement contextWidthElement, bool explicitWidth)
        {
            if (explicitWidth || inspectorElement == null)
            {
                return;
            }

            if (field.Q<Label>(className: BaseField<object>.labelUssClassName) is not { } label)
            {
                return;
            }

            const float labelExtraPadding = 37.0f;
            const float labelBaseMinWidth = 123.0f;
            const float labelExtraContextWidth = 1.0f;
            const float labelWidthRatio = 0.45f;

            var spacing = field.worldBound.x - inspectorElement.worldBound.x -
                          inspectorElement.resolvedStyle.paddingLeft;

            if (float.IsNaN(spacing))
            {
                return;
            }

            var totalPadding = labelExtraPadding + spacing + field.resolvedStyle.paddingLeft;
            var minWidth = labelBaseMinWidth - spacing - field.resolvedStyle.paddingLeft;
            var widthElement = contextWidthElement ?? inspectorElement;

            label.style.minWidth = Mathf.Max(minWidth, 0);
            
            var newWidth = (widthElement.resolvedStyle.width + labelExtraContextWidth) * labelWidthRatio -
                           totalPadding;
            label.style.width = Mathf.Max(0f, newWidth);
        }
    }
}
