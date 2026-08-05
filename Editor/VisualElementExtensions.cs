using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TriInspector
{
    public static class VisualElementExtensions
    {
        public const int PollIntervalMs = 100;

        public static T FindAncestor<T>(this VisualElement element) where T : VisualElement
        {
            for (var current = element.parent; current != null; current = current.parent)
            {
                if (current is T result)
                {
                    return result;
                }
            }

            return null;
        }

        public static void PeriodicRun(this VisualElement element, Action action)
        {
            element.schedule.Execute(action).Every(PollIntervalMs);
        }

        public static void AutoSyncLabelFromProperty(this Foldout foldout, TriProperty property)
        {
            AutoSyncLabelFromProperty(foldout, property, text => foldout.text = text);
        }

        public static void AutoSyncLabelFromProperty<T>(this BaseField<T> field, TriProperty property)
        {
            AutoSyncLabelFromProperty(field, property, text => field.label = text);
        }

        public static void AutoSyncLabelFromProperty(this PropertyField field, TriProperty property)
        {
            AutoSyncLabelFromProperty(field, property, text => field.label = text);
        }

        private static void AutoSyncLabelFromProperty(VisualElement el, TriProperty property, Action<string> setText)
        {
            void Sync()
            {
                var name = property.DisplayNameContent;
                setText(name.text);
            }

            PeriodicRun(el, Sync);
            Sync();
        }

        public static void AutoSyncValueFromProperty<T>(this BaseField<T> field, TriProperty property)
        {
            field.AutoSyncValueFromProperty(property, () => (T) property.Value);
        }

        public static void AutoSyncValueFromProperty<T>(this BaseField<T> field, TriProperty property, Func<T> getValue)
        {
            void Sync(TriProperty _)
            {
                field.showMixedValue = property.IsValueMixed;

                // Don't clobber the value while the user is editing it.
                if (field.IsBeingEdited())
                {
                    return;
                }

                var current = getValue();
                if (!EqualityComparer<T>.Default.Equals(field.value, current))
                {
                    field.SetValueWithoutNotify(current);
                }
            }

            field.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                property.ValueChanged += Sync;
                Sync(property);
            });

            field.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                property.ValueChanged -= Sync;
            });
        }

        private static bool IsBeingEdited(this VisualElement field)
        {
            var focused = field.focusController?.focusedElement as VisualElement;
            return focused != null && (focused == field || field.Contains(focused));
        }
    }
}