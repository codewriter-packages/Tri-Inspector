using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using TriInspector.Drawers;
using TriInspector.Elements;
using TriInspector.Utilities;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(TableListDrawer), TriDrawerOrder.Drawer)]

namespace TriInspector.Drawers
{
    public class TableListDrawer : TriAttributeDrawer<TableListAttribute>
    {
        public override string CanDraw(TriProperty property)
        {
            if (property.PropertyType != TriPropertyType.Array)
            {
                return "[TableList] valid only on lists";
            }

            return null;
        }

        public override TriElement CreateElement(TriProperty property, TriElement next)
        {
            return new TableElement(property);
        }

        private class TableElement : TriElement
        {
            private readonly TableMultiColumnTreeView _treeView;

            public TableElement(TriProperty property)
            {
                _treeView = new TableMultiColumnTreeView(property, this);
            }

            public override float GetHeight(float width)
            {
                return _treeView.totalHeight;
            }

            public override void OnGUI(Rect position)
            {
                _treeView.OnGUI(position);
            }
        }

        [Serializable]
        private class TableMultiColumnTreeView : TreeView
        {
            private readonly TriProperty _property;
            private readonly TriElement _cellElementContainer;
            private readonly Dictionary<string, int> _cellIndexByName;
            private readonly Dictionary<TriProperty, TriElement> _cellElements;
            private readonly TableListPropertyOverrideContext _propertyOverrideContext;

            public TableMultiColumnTreeView(TriProperty property, TriElement container)
                : base(new TreeViewState(), BuildHeader(property))
            {
                _property = property;
                _cellElementContainer = container;

                _cellIndexByName = new Dictionary<string, int>();
                _cellElements = new Dictionary<TriProperty, TriElement>();

                _propertyOverrideContext = new TableListPropertyOverrideContext();

                rowHeight = 20;
                showAlternatingRowBackgrounds = true;
                showBorder = true;
                useScrollView = false;

                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem(0, -1, string.Empty);

                for (var index = 0; index < _property.ArrayElementProperties.Count; index++)
                {
                    var rowChildProperty = _property.ArrayElementProperties[index];
                    root.AddChild(new TableTreeElement(index, rowChildProperty));

                    foreach (var cellValueProperty in rowChildProperty.ChildrenProperties)
                    {
                        if (!_cellIndexByName.ContainsKey(cellValueProperty.RawName))
                        {
                            _cellIndexByName.Add(cellValueProperty.RawName, _cellIndexByName.Count);
                        }
                    }
                }

                if (root.children == null)
                {
                    root.AddChild(new TreeViewItem(0, 0, "Empty"));
                }

                return root;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                var tableItem = args.item as TableTreeElement;

                if (tableItem == null)
                {
                    base.RowGUI(args);
                    return;
                }

                foreach (var cellValueProperty in tableItem.Property.ChildrenProperties)
                {
                    if (!_cellIndexByName.TryGetValue(cellValueProperty.RawName, out var cellIndex))
                    {
                        continue;
                    }

                    var cellRect = args.GetCellRect(cellIndex);

                    if (!_cellElements.ContainsKey(cellValueProperty))
                    {
                        var cellElement = new TriPropertyElement(cellValueProperty, new TriPropertyElement.Props
                        {
                            forceInline = true,
                        });
                        _cellElements.Add(cellValueProperty, cellElement);
                        _cellElementContainer.AddChild(cellElement);
                    }

                    using (TriPropertyOverrideContext.BeginOverride(_propertyOverrideContext))
                    {
                        _cellElements[cellValueProperty].OnGUI(cellRect);
                    }
                }
            }

            private static MultiColumnHeader BuildHeader(TriProperty property)
            {
                var columns = TriTypeDefinition
                    .GetCached(property.ArrayElementType)
                    .Properties
                    .Select(it => new MultiColumnHeaderState.Column
                    {
                        headerContent = new GUIContent(ObjectNames.NicifyVariableName(it.Name)),
                        autoResize = true,
                        canSort = false,
                        allowToggleVisibility = false,
                    })
                    .Concat(new[]
                    {
                        new MultiColumnHeaderState.Column
                        {
                            headerContent = GUIContent.none,
                            autoResize = false,
                            canSort = false,
                            allowToggleVisibility = false,
                            width = 10,
                        },
                    })
                    .ToArray();

                var header = new MultiColumnHeader(new MultiColumnHeaderState(columns))
                {
                    canSort = false,
                };

                header.ResizeToFit();

                return header;
            }
        }

        [Serializable]
        private class TableTreeElement : TreeViewItem
        {
            public TableTreeElement(int id, TriProperty property) : base(id, 0)
            {
                Property = property;
            }

            public TriProperty Property { get; }
        }

        private class TableListPropertyOverrideContext : TriPropertyOverrideContext
        {
            public override GUIContent GetDisplayName(TriProperty property)
            {
                return GUIContent.none;
            }
        }
    }
}