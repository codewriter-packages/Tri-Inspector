using System;
using System.Collections.Generic;
using System.Reflection;
using TriInspector.Resolvers;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector
{
    public static class VisualElementExtensions
    {
        private static readonly FieldInfo ToggleClickable;
        private static readonly PropertyInfo ClickableSetAcceptClicksIfDisabled;

        public const int PollIntervalMs = 100;

        static VisualElementExtensions()
        {
            ToggleClickable = typeof(Toggle).GetField("m_Clickable", BindingFlags.Instance | BindingFlags.NonPublic);
            ClickableSetAcceptClicksIfDisabled = typeof(Clickable).GetProperty("acceptClicksIfDisabled",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public static T FindAncestor<T>(this VisualElement element) where T : VisualElement
        {
            for (var current = element; current != null; current = current.parent)
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

        public static void TrackResolvedValue<T>(this VisualElement el,
            TriProperty property, ValueResolver<T> resolver, T defaultValue, Action<T> callback)
        {
            el.PeriodicRun(() =>
            {
                try
                {
                    callback(resolver.GetValue(property, defaultValue));
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            });
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
                try
                {
                    setText(name.text);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            PeriodicRun(el, Sync);
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


        public static void SetAcceptClicksIfDisabled(this Foldout foldout, bool value)
        {
            if (foldout != null)
            {
                SetAcceptClicksIfDisabled(foldout.Q<Toggle>(), value);
            }
        }

        public static void SetAcceptClicksIfDisabled(this Toggle toggle, bool value)
        {
            if (toggle != null)
            {
                SetAcceptClicksIfDisabled(ToggleClickable.GetValue(toggle) as Clickable, value);
            }
        }

        public static void SetAcceptClicksIfDisabled(this Clickable clickable, bool value)
        {
            if (clickable != null)
            {
                ClickableSetAcceptClicksIfDisabled?.SetValue(clickable, value);
            }
        }
    }
}