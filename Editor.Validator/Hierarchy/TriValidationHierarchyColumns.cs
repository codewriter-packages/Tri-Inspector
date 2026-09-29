#if UNITY_6000_6_OR_NEWER

using System;
using TriInspector.Utilities;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.Validator
{
    internal static class TriValidationHierarchyColumns
    {
        private const string ErrorColumnId = "TriInspector/Errors";
        private const string WarningColumnId = "TriInspector/Warnings";

        private static readonly Color ErrorColor = new(0.86f, 0.27f, 0.22f);
        private static readonly Color WarningColor = new(0.90f, 0.70f, 0.0f);
        private static readonly Color ChipBackground = new(0f, 0f, 0f, 0.2f);

        [HierarchyViewColumnDescriptor(ErrorColumnId)]
        internal static void CreateErrorColumn(HierarchyViewColumnDescriptor desc)
        {
            desc.Icon = TriMessageType.Error.GetIcon();
            desc.Tooltip = "Tri Inspector validation errors";
            desc.DefaultPriority = 900;
            desc.DefaultWidth = 25;
        }

        [HierarchyViewColumnDescriptor(WarningColumnId)]
        internal static void CreateWarningColumn(HierarchyViewColumnDescriptor desc)
        {
            desc.Icon = TriMessageType.Warning.GetIcon();
            desc.Tooltip = "Tri Inspector validation warnings";
            desc.DefaultPriority = 899;
            desc.DefaultWidth = 25;
        }

        [HierarchyViewCellDescriptor(ErrorColumnId, typeof(HierarchyGameObjectHandler))]
        internal static void BindErrorCell(HierarchyViewCellDescriptor desc)
        {
            BindCell(desc, TriMessageType.Error);
        }

        [HierarchyViewCellDescriptor(WarningColumnId, typeof(HierarchyGameObjectHandler))]
        internal static void BindWarningCell(HierarchyViewCellDescriptor desc)
        {
            BindCell(desc, TriMessageType.Warning);
        }

        internal static void BindCell(HierarchyViewCellDescriptor desc, TriMessageType type)
        {
            desc.BindCell = cell =>
            {
                var chip = cell.Q<CounterChip>();
                if (chip == null)
                {
                    chip = MakeChip(cell, type, has => cell.IsDefaultValue = !has);
                }

                var handler = (HierarchyGameObjectHandler) cell.Handler;
                chip.SetGoEntityID((TriEntityID) handler.GetEntityId(cell.Node));
            };
        }

        private static CounterChip MakeChip(VisualElement cell, TriMessageType severity, Action<bool> setDefault)
        {
            cell.style.alignItems = Align.Center;
            cell.style.justifyContent = Justify.Center;

            var chip = new CounterChip(severity, setDefault);
            cell.Add(chip);
            return chip;
        }

        private sealed class CounterChip : Label
        {
            private readonly TriMessageType _severity;
            private readonly Action<bool> _setDefault;
            private TriEntityID _goEntityId;

            public CounterChip(TriMessageType severity, Action<bool> setDefault)
            {
                _severity = severity;
                _setDefault = setDefault;

                style.unityTextAlign = TextAnchor.MiddleCenter;
                style.unityFontStyleAndWeight = FontStyle.Bold;
                style.marginTop = 1;
                style.marginBottom = 1;
                style.paddingLeft = 6;
                style.paddingRight = 6;
                style.paddingTop = 1;
                style.paddingBottom = 1;
                style.backgroundColor = ChipBackground;
                style.borderTopLeftRadius = 6;
                style.borderTopRightRadius = 6;
                style.borderBottomLeftRadius = 6;
                style.borderBottomRightRadius = 6;
                style.color = severity == TriMessageType.Error ? ErrorColor : WarningColor;

                RegisterCallback<AttachToPanelEvent>(_ => OnAttach());
                RegisterCallback<DetachFromPanelEvent>(_ => OnDetach());
            }

            public void SetGoEntityID(TriEntityID id)
            {
                _goEntityId = id;
                Refresh();
            }

            private void OnAttach()
            {
                TriValidationHierarchyCounts.Changed += Refresh;
                Refresh();
            }

            private void OnDetach()
            {
                TriValidationHierarchyCounts.Changed -= Refresh;
            }

            private void Refresh()
            {
                var count = 0;
                if (TriValidationHierarchyCounts.TryGet(_goEntityId, out var errors, out var warnings))
                {
                    count = _severity == TriMessageType.Error ? errors : warnings;
                }

                var hasCount = count > 0;
                style.display = hasCount ? DisplayStyle.Flex : DisplayStyle.None;
                if (hasCount)
                {
                    text = count.ToString();
                }

                _setDefault?.Invoke(hasCount);
            }
        }
    }
}

#endif