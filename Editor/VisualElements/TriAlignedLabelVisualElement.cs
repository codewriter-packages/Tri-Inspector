using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriAlignedLabelForGenericVisualElement : TriAlignedLabelVisualElement<object>
    {
        public Foldout Foldout { get; }

        public TriAlignedLabelForGenericVisualElement(TriProperty property, VisualElement content,
            bool collapsible = false)
            : base(string.IsNullOrEmpty(property.DisplayName) ? null : " ", content)
        {
            AddToClassList(TriStyles.TriAlignedGeneric);

            Foldout = new Foldout
            {
                toggleOnLabelClick = collapsible,
                value = !collapsible || property.IsExpanded,
            };

            if (collapsible)
            {
                Foldout.SetAcceptClicksIfDisabled(true);
                AddToClassList(TriStyles.TriAlignedGenericCollapsible);
            }
            else
            {
                AddToClassList(TriStyles.TriAlignedGenericNonCollapsible);
            }

            Foldout.AutoSyncLabelFromProperty(property);

            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                Foldout.BindProperty(serializedProperty);
            }

            labelElement.Add(Foldout);
        }
    }

    public class TriAlignedLabelVisualElement<T> : BaseField<T>
    {
        public TriAlignedLabelVisualElement(TriProperty property, VisualElement content)
            : this(property.DisplayName, content)
        {
            // This BaseField is only a layout wrapper around arbitrary content; its value is never
            // read or written. We bind it to the serialized property purely so Unity draws the native
            // prefab-override bar / context menu against the aligned label.
            if (property.TryGetSerializedProperty(out var serializedProperty) &&
                serializedProperty.propertyType != SerializedPropertyType.ManagedReference)
            {
                this.BindProperty(serializedProperty);
            }

            this.AutoSyncLabelFromProperty(property);
        }

        protected TriAlignedLabelVisualElement(string label, VisualElement content)
            : base(label, content)
        {
            content.AddToClassList(TriStyles.TriAlignedLabelContent);

            TriLabelWidthContextVisualElement.SetupAlignedLabel(this);
        }
    }

    public static class TriAlignedLabelVisualElement
    {
        public static VisualElement Create(TriProperty property, VisualElement child)
        {
            if (property.TryGetSerializedProperty(out var serializedProperty))
            {
                return serializedProperty.propertyType switch
                {
                    SerializedPropertyType.Integer => new TriAlignedLabelVisualElement<int>(property, child),
                    SerializedPropertyType.Boolean => new TriAlignedLabelVisualElement<bool>(property, child),
                    SerializedPropertyType.Float => new TriAlignedLabelVisualElement<float>(property, child),
                    SerializedPropertyType.String => new TriAlignedLabelVisualElement<string>(property, child),
                    SerializedPropertyType.Color => new TriAlignedLabelVisualElement<Color>(property, child),
                    SerializedPropertyType.ObjectReference => new TriAlignedLabelVisualElement<Object>(property, child),
                    SerializedPropertyType.LayerMask => new TriAlignedLabelVisualElement<int>(property, child),
                    SerializedPropertyType.Enum => new TriAlignedLabelVisualElement<int>(property, child),
                    SerializedPropertyType.Vector2 => new TriAlignedLabelVisualElement<Vector2>(property, child),
                    SerializedPropertyType.Vector3 => new TriAlignedLabelVisualElement<Vector3>(property, child),
                    SerializedPropertyType.Vector4 => new TriAlignedLabelVisualElement<Vector4>(property, child),
                    SerializedPropertyType.Rect => new TriAlignedLabelVisualElement<Rect>(property, child),
                    SerializedPropertyType.ArraySize => new TriAlignedLabelVisualElement<int>(property, child),
                    SerializedPropertyType.Character => new TriAlignedLabelVisualElement<string>(property, child),
                    SerializedPropertyType.AnimationCurve => new TriAlignedLabelVisualElement<AnimationCurve>(property, child),
                    SerializedPropertyType.Bounds => new TriAlignedLabelVisualElement<Bounds>(property, child),
                    SerializedPropertyType.Gradient => new TriAlignedLabelVisualElement<Gradient>(property, child),
                    SerializedPropertyType.Quaternion => new TriAlignedLabelVisualElement<Quaternion>(property, child),
                    SerializedPropertyType.ExposedReference => new TriAlignedLabelVisualElement<Object>(property, child),
                    SerializedPropertyType.FixedBufferSize => new TriAlignedLabelVisualElement<int>(property, child),
                    SerializedPropertyType.Vector2Int => new TriAlignedLabelVisualElement<Vector2Int>(property, child),
                    SerializedPropertyType.Vector3Int => new TriAlignedLabelVisualElement<Vector3Int>(property, child),
                    SerializedPropertyType.RectInt => new TriAlignedLabelVisualElement<RectInt>(property, child),
                    SerializedPropertyType.BoundsInt => new TriAlignedLabelVisualElement<BoundsInt>(property, child),
                    SerializedPropertyType.Hash128 => new TriAlignedLabelVisualElement<Hash128>(property, child),
                    _ => new TriAlignedLabelVisualElement<object>(property, child),
                };
            }

            return new TriAlignedLabelVisualElement<object>(property, child);
        }
    }
}