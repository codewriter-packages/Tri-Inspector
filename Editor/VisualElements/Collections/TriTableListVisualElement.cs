using System;
using System.Collections.Generic;
using TriInspector.Resolvers;
using TriInspector.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TriInspector.VisualElements
{
    public class TriTableListVisualElement : TriCollectionVisualElement
    {
        private static readonly string[] SingleColumnTitles = {"Element"};

        private readonly TriProperty _property;
        private readonly ValueResolver<string>[] _headerResolvers;
        private readonly float[] _sizes;
        private readonly VisualElement _columnsRow = new VisualElement();

        public TriTableListVisualElement(TriProperty property, ValueResolver<string>[] headerResolvers = null,
            float[] sizes = null)
            : base(property)
        {
            _property = property;
            _headerResolvers = headerResolvers;
            _sizes = sizes;

            _columnsRow.AddToClassList(TriStyles.TableHeaderColumns);
            _columnsRow.EnableInClassList(TriStyles.TableHeaderReorderable, reorderable);

            AddToClassList(TriStyles.Table);

            var labelOverride = new TableListPropertyOverrideContext(property);
            RegisterCallback<AttachToPanelEvent>(_ => property.PropertyTree.AddPropertyOverride(labelOverride));
            RegisterCallback<DetachFromPanelEvent>(_ => property.PropertyTree.RemovePropertyOverride(labelOverride));
        }

        protected override VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.AddToClassList(TriStyles.TableHeader);
            header.Add(base.CreateHeader());
            header.Add(_columnsRow);
            return header;
        }

        private void EnsureHeaderColumns(TriArray<string> titles)
        {
            if (_columnsRow.childCount != 0)
            {
                return;
            }

            var cellsContainer = new VisualElement();
            cellsContainer.style.flexDirection = FlexDirection.Row;
            cellsContainer.style.flexGrow = 1;
            cellsContainer.style.flexBasis = 0;

            for (var i = 0; i < titles.Count; i++)
            {
                var title = titles[i];

                var cell = new Label(title);
                cell.AddToClassList(TriStyles.TableHeaderCell);
                TriColumnSizes.Apply(cell, _sizes, i);

                if (_headerResolvers != null && i < _headerResolvers.Length && _headerResolvers[i] != null)
                {
                    cell.TrackResolvedValue(_property, _headerResolvers[i], title, value => cell.text = value);
                }

                cellsContainer.Add(cell);
            }

            _columnsRow.Add(cellsContainer);

            if (allowRemove)
            {
                var spacer = new VisualElement();
                spacer.AddToClassList(TriStyles.TableHeaderRemoveSpacer);
                _columnsRow.Add(spacer);
            }

            UpdateColumnsRowDisplay();
        }

        private void UpdateColumnsRowDisplay()
        {
            _columnsRow.style.display = _property.IsExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void SetExpanded(bool expanded)
        {
            base.SetExpanded(expanded);

            UpdateColumnsRowDisplay();
        }

        protected override VisualElement CreateItemElement(TriProperty property)
        {
            if (property.PropertyType == TriPropertyType.Generic)
            {
                var content = new TableRowVisualElement(property.ValueType, property.ChildrenProperties, _sizes);
                content.AddToClassList(TriStyles.TableRow);
                EnsureHeaderColumns(content.ColumnTitles);
                return new TriValidationResultsVisualElement(property, content);
            }

            EnsureHeaderColumns(SingleColumnTitles);

            var row = new VisualElement();
            row.AddToClassList(TriStyles.TableRow);
            row.Add(CreateCell(new TriPropertyVisualElement(property, new TriPropertyVisualElement.Props
            {
                forceInline = true,
            }), 0, _sizes));

            return row;
        }

        private static VisualElement CreateCell(VisualElement content, int columnIndex, float[] sizes)
        {
            var cell = new VisualElement();
            cell.AddToClassList(TriStyles.TableCell);
            cell.AddToClassList(TriStyles.UnityInspectorElement);
            cell.AddToClassList(TriStyles.UnityInspectorMainContainer);
            cell.AddToClassList(TriStyles.TriInspectorElement);
            TriColumnSizes.Apply(cell, sizes, columnIndex);
            cell.Add(content);
            return cell;
        }

        private class TableRowVisualElement : TriPropertyCollectionVisualElement
        {
            private readonly float[] _sizes;
            private int _columnIndex;

            public TableRowVisualElement(Type declarationsType, TriArray<TriProperty> properties, float[] sizes)
                : base(declarationsType)
            {
                _sizes = sizes;
                ColumnTitles = new List<string>();

                foreach (var property in properties)
                {
                    var columnCount = childCount;
                    AddProperty(property, new TriPropertyVisualElement.Props {forceInline = true}, out var group);

                    if (childCount != columnCount)
                    {
                        ColumnTitles.Add(group ?? ObjectNames.NicifyVariableName(property.RawName));
                    }
                }
            }

            public List<string> ColumnTitles { get; }

            protected override void AddPropertyChild(VisualElement child, TriProperty property)
            {
                base.AddPropertyChild(CreateCell(child, _columnIndex++, _sizes), property);
            }
        }

        private class TableListPropertyOverrideContext : TriPropertyOverrideContext
        {
            private readonly TriProperty _listProperty;
            private readonly GUIContent _noneLabel = GUIContent.none;

            public TableListPropertyOverrideContext(TriProperty listProperty)
            {
                _listProperty = listProperty;
            }

            public override bool TryGetDisplayName(TriProperty property, out GUIContent displayName)
            {
                if (property.PropertyType == TriPropertyType.Primitive &&
                    property.Parent?.Parent == _listProperty &&
                    !property.TryGetAttribute(out GroupAttribute _))
                {
                    displayName = _noneLabel;
                    return true;
                }

                displayName = default;
                return false;
            }
        }
    }
}