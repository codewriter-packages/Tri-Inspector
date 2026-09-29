using System;
using System.Collections.Generic;
using TriInspector.Utilities;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TriInspector.Validator
{
    public sealed class TriValidationTreeVisualElement : VisualElement
    {
        private readonly MultiColumnTreeView _treeView;

        public TriValidationTreeVisualElement()
        {
            _treeView = new MultiColumnTreeView(new Columns
            {
                new Column
                {
                    name = "name",
                    title = "Name",
                    stretchable = true,
                    sortable = false,
                    makeCell = MakeNameCell,
                    bindCell = BindNameCell,
                },
                new Column
                {
                    name = "Path",
                    title = "Path",
                    width = 40f,
                    minWidth = 40f,
                    sortable = false,
                    makeCell = MakePathCell,
                    bindCell = BindPathCell,
                },
                MakeCounterColumn("Warnings", TriMessageType.Warning, TriValidationStyles.CounterCellWarning),
                MakeCounterColumn("Errors", TriMessageType.Error, TriValidationStyles.CounterCellError),
            })
            {
                fixedItemHeight = 20,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
            };
            _treeView.AddToClassList(TriValidationStyles.Tree);
            _treeView.selectionChanged += OnSelectionChanged;
            _treeView.itemsChosen += OnItemsChosen;
            Add(_treeView);
        }

        public event Action<Object> ItemSelected;
        public event Action<Object> ItemChosen;

        public BaseTreeView TreeView => _treeView;

        private VisualElement MakeNameCell()
        {
            return new TriValidationItemVisualElement();
        }

        private void BindNameCell(VisualElement element, int index)
        {
            if (element is TriValidationItemVisualElement itemElement)
            {
                itemElement.Bind(_treeView.GetItemDataForIndex<TriValidationNodeData>(index));
            }
        }

        private static VisualElement MakePathCell()
        {
            var label = new Label();
            label.AddToClassList(TriValidationStyles.PathCell);
            return label;
        }

        private void BindPathCell(VisualElement element, int index)
        {
            if (element is Label label)
            {
                var data = _treeView.GetItemDataForIndex<TriValidationNodeData>(index);
                var path = data.kind == TriValidationNodeKind.Issue ? data.propertyPath ?? string.Empty : "";
                label.text = path;
                label.tooltip = path;
            }
        }

        private Column MakeCounterColumn(string columnName, TriMessageType severity, string cellClass)
        {
            var isError = severity == TriMessageType.Error;

            return new Column
            {
                name = columnName,
                width = 35f,
                minWidth = 35f,
                sortable = false,
                makeHeader = () =>
                {
                    var image = new Image {image = severity.GetIcon()};
                    image.AddToClassList(TriValidationStyles.CounterHeaderIcon);
                    return image;
                },
                makeCell = () =>
                {
                    var cell = new VisualElement();
                    cell.AddToClassList(TriValidationStyles.CounterCell);

                    var chip = new Label();
                    chip.AddToClassList(TriValidationStyles.CounterChip);
                    chip.AddToClassList(cellClass);
                    cell.Add(chip);

                    return cell;
                },
                bindCell = (element, index) =>
                {
                    var data = _treeView.GetItemDataForIndex<TriValidationNodeData>(index);
                    var count = isError ? data.errorCount : data.warningCount;
                    var chip = (Label) element[0];

                    var show = data.kind != TriValidationNodeKind.Issue && count > 0;
                    chip.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                    if (show)
                    {
                        chip.text = count.ToString();
                    }
                },
            };
        }

        private void OnSelectionChanged(IEnumerable<object> selection) => Raise(selection, ItemSelected);
        private void OnItemsChosen(IEnumerable<object> selection) => Raise(selection, ItemChosen);

        private static void Raise(IEnumerable<object> selection, Action<Object> callback)
        {
            foreach (var item in selection)
            {
                if (item is TriValidationNodeData data && data.target != null)
                {
                    callback?.Invoke(data.target);
                }

                break;
            }
        }
    }
}